using SigningService.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;

namespace SigningService.Signers.StrongName
{
    internal class StrongNameSignerHelper
    {
        private Stream _peStream;

        private bool _strongNameSignedBitSet = false;
        private bool _strongNameSignedBitOverwritten = false;

        // Lazy fields
        private Lazy<HashAlgorithm> _hashAlgorithm;
        private Lazy<AssemblyMetadataExtractor> _dataExtractor;
        private Lazy<List<HashingBlock>> _hashingBlocks;

        public StrongNameSignerHelper(Stream peStream)
        {
            _peStream = peStream;
            _dataExtractor = new Lazy<AssemblyMetadataExtractor>(InitDataExtractor);
            _hashAlgorithm = new Lazy<HashAlgorithm>(InitHashAlgorithm);
            _hashingBlocks = new Lazy<List<HashingBlock>>(InitHashingBlocks);
        }

#region StrongNameSignature
        public bool CanEmbedSignature { get { return CanHash & _peStream.CanWrite; } }
        public bool HasStrongNameSignature
        {   
            get
            {
                if (!HasStrongNameSignatureDirectory)
                {
                    return false;
                }

                return StrongNameSignedFlag;
            }
        }
        public bool HasStrongNameSignatureDirectory { get { return _dataExtractor.Value.HasStrongNameSignatureDirectory; } }
        public int StrongNameSignatureSize { get { return _dataExtractor.Value.StrongNameSignatureDirectorySize; } }
        public byte[] StrongNameSignature { get { return GetStrongNameSignature(); } set { SetStrongNameSignature(value); } }
        private void SetStrongNameSignature(byte[] signature)
        {
            if (signature == null)
            {
                throw new ArgumentNullException("signature");
            }

            AssemblyMetadataExtractor dataExtractor = _dataExtractor.Value;

            if (!HasStrongNameSignatureDirectory)
            {
                ExceptionsHelper.ThrowNoStrongNameSignatureDirectory();
                return;
            }

            if (dataExtractor.StrongNameSignatureDirectorySize != signature.Length)
            {
                ExceptionsHelper.ThrowStrongNameSignatureDirectorySizeIsDifferentThanProvidedSignature(dataExtractor.StrongNameSignatureDirectorySize, signature.Length);
                return;
            }

            ExceptionsHelper.ThrowIfStreamNotWritable(_peStream);

            _peStream.Seek(dataExtractor.StrongNameSignatureDirectoryOffset, SeekOrigin.Begin);
            _peStream.Write(signature, 0, signature.Length);
        }
        private byte[] GetStrongNameSignature()
        {
            AssemblyMetadataExtractor dataExtractor = _dataExtractor.Value;
            if (!HasStrongNameSignatureDirectory)
            {
                ExceptionsHelper.ThrowNoStrongNameSignatureDirectory();
                return null;
            }

            _peStream.Seek(dataExtractor.StrongNameSignatureDirectoryOffset, SeekOrigin.Begin);
            int left = dataExtractor.StrongNameSignatureDirectorySize;
            byte[] signature = new byte[left];
            while (left > 0)
            {
                int bytesRead = _peStream.Read(signature, signature.Length - left, signature.Length);
                if (bytesRead <= 0)
                {
                    ExceptionsHelper.ThrowUnexpectedEndOfStream(_peStream.Position);
                    return null;
                }
                left -= bytesRead;
            }

            return signature;
        }

        public void EmbedEmptyStrongNameSignature()
        {
            AssemblyMetadataExtractor dataExtractor = _dataExtractor.Value;
            byte[] signature = new byte[dataExtractor.StrongNameSignatureDirectorySize];
            SetStrongNameSignature(signature);
        }
        public void RemoveStrongNameSignature()
        {
            StrongNameSignedFlag = false;
            EmbedEmptyStrongNameSignature();
        }
#endregion

#region StrongNameSigned flag
        public bool StrongNameSignedFlag { get { return GetStrongNameSignedFlag(); } set { SetStrongNameSignedFlag(_peStream, value); } }

        private bool GetStrongNameSignedFlag()
        {
            AssemblyMetadataExtractor dataExtractor = _dataExtractor.Value;
            return _strongNameSignedBitOverwritten ? _strongNameSignedBitSet : dataExtractor.HasStrongNameSignedFlag;
        }

        private void SetStrongNameSignedFlag(Stream writablePEStream, bool value)
        {
            ExceptionsHelper.ThrowIfStreamNotWritable(writablePEStream);

            AssemblyMetadataExtractor dataExtractor = _dataExtractor.Value;

            using (BinaryWriter bw = new BinaryWriter(writablePEStream, Encoding.ASCII, leaveOpen : true))
            {
                bw.Seek(dataExtractor.CorFlagsOffset, SeekOrigin.Begin);
                CorFlags corFlags = dataExtractor.CorFlagsValue;
                if (value)
                {
                    corFlags |= CorFlags.StrongNameSigned;
                }
                else
                {
                    corFlags &= ~(CorFlags.StrongNameSigned);
                }
                bw.Write((UInt32)(corFlags));
            }

            if (writablePEStream == _peStream)
            {
                _strongNameSignedBitSet = value;
                _strongNameSignedBitOverwritten = true;
            }
        }
#endregion

#region Hashing
        public bool CanHash
        {
            get
            {
                AssemblyMetadataExtractor dataExtractor = _dataExtractor.Value;
                bool ret = _peStream.CanSeek && _peStream.CanRead;
                ret &= dataExtractor.HasStrongNameSignatureDirectory;
                ret &= _hashAlgorithm.Value != null;
                return ret;
            }
        }

        public bool CanSign
        {
            get
            {
                return CanHash && CanEmbedSignature && VerifyCounterSignature() && !StrongNameSignedFlag;
            }
        }

        public byte[] ComputeHash()
        {
            if (!CanHash)
            {
                ExceptionsHelper.ThrowAssemblyNotHashable();
            }

            using (MemoryStream ms = new MemoryStream())
            {
                _peStream.Seek(0, SeekOrigin.Begin);
                _peStream.CopyTo(ms);
                return PrepareForSigningAndComputeHash(ms);
            }
        }

        public byte[] PrepareForSigningAndComputeHash()
        {
            return PrepareForSigningAndComputeHash(_peStream);
        }

        private byte[] PrepareForSigningAndComputeHash(Stream writablePEStream)
        {
            ExceptionsHelper.ThrowIfStreamNotWritable(writablePEStream);

            PrepareForSigning(writablePEStream);
            return HashingHelpers.CalculateAssemblyHash(writablePEStream, _hashAlgorithm.Value, _hashingBlocks.Value);
        }

        private void PrepareForSigning(Stream writablePEStream)
        {
            SetStrongNameSignedFlag(writablePEStream, true);
            EraseChecksum(writablePEStream);
        }
#endregion

#region Public Key Blobs
        public bool HasUniqueSignatureAndIdentityPublicKeyBlobs { get { return AssemblySignatureKeyAttributePublicKeyBlob != null; } }
        public PublicKeyBlob SignaturePublicKeyBlob
        {
            get
            {
                if (HasUniqueSignatureAndIdentityPublicKeyBlobs)
                {
                    return AssemblySignatureKeyAttributePublicKeyBlob;
                }
                else
                {
                    return AssemblyDefinitionPublicKeyBlob;
                }
            }
        }
        public PublicKeyBlob IdentityPublicKeyBlob { get { return AssemblyDefinitionPublicKeyBlob; } }
        public PublicKeyBlob AssemblyDefinitionPublicKeyBlob { get { return _dataExtractor.Value.AssemblyDefinitionPublicKeyBlob; } }
        public PublicKeyBlob AssemblySignatureKeyAttributePublicKeyBlob { get { return _dataExtractor.Value.AssemblySignatureKeyAttributePublicKeyBlob; } }

        public byte[] CounterSignature { get { return _dataExtractor.Value.AssemblySignatureKeyAttributeCounterSignatureBlob; } }
        public bool VerifyCounterSignature()
        {
            if (HasUniqueSignatureAndIdentityPublicKeyBlobs)
            {
                return IdentityPublicKeyBlob.VerifyData(SignaturePublicKeyBlob.Blob, CounterSignature);
            }

            // If no counter signature then verification passes since there is nothing to verify
            return true;
        }

#endregion

#region Lazy fields initializers
        /// <summary>
        /// Initializes lazy private field _dataExtractor
        /// </summary>
        /// <returns>Data extractor for PE file with CLI metadata</returns>
        private AssemblyMetadataExtractor InitDataExtractor()
        {
            return new AssemblyMetadataExtractor(_peStream);
        }

        /// <summary>
        /// Initializes lazy private field _hashAlgorithm
        /// </summary>
        /// <returns>Instance of the System.Security.Cryptography.HashAlgorithm related to signature public key</returns>
        private HashAlgorithm InitHashAlgorithm()
        {
            return HashingHelpers.CreateHashAlgorithm(SignaturePublicKeyBlob.HashAlgorithm);
        }

        /// <summary>
        /// Initializes lazy private field _hashingBlocks which decides what parts of PE will be hashed
        /// </summary>
        private List<HashingBlock> InitHashingBlocks()
        {
            AssemblyMetadataExtractor dataExtractor = _dataExtractor.Value;

            List<HashingBlock> hashingBlocks = new List<HashingBlock>(16);

            hashingBlocks.Add(new HashingBlock(HashingBlockHashing.Hash, "PE Headers", 0, dataExtractor.PaddingBetweenTheSectionHeadersAndSectionsOffset));
            foreach (var section in dataExtractor.SectionsInfo)
            {
                string name = string.Format("Section {0}", section.Name);
                hashingBlocks.Add(new HashingBlock(HashingBlockHashing.Hash, name, section.Offset, section.Size));
            }

            hashingBlocks.Add(new HashingBlock(HashingBlockHashing.HashZeros, "Checksum", dataExtractor.ChecksumOffset, dataExtractor.ChecksumSize));

            // According to ECMA-335 this block should be zeroed and hashed (HashingBlockHashing.HashZeros)
            // For compat reasons we fully hash it
            //specialHashingBlocks.Add(new HashingBlock(HashingBlockHashing.Hash, "StrongNameSignatureDirectory header", StrongNameSignatureDirectoryHeaderOffset, StrongNameSignatureDirectoryHeaderSize));
            
            hashingBlocks.Add(new HashingBlock(HashingBlockHashing.HashZeros, "CertificateTableDirectory header", dataExtractor.CertificateTableDirectoryHeaderOffset, dataExtractor.CertificateTableDirectoryHeaderSize));

            if (dataExtractor.HasStrongNameSignatureDirectory)
            {
                hashingBlocks.Add(new HashingBlock(HashingBlockHashing.Skip, "StrongNameSignatureDirectory", dataExtractor.StrongNameSignatureDirectoryOffset, dataExtractor.StrongNameSignatureDirectorySize));
            }
            else
            {
                ExceptionsHelper.ThrowNoStrongNameSignatureDirectory();
            }

            // In theory we should be hashing it, in practice in some cases it might be past the last section
            // and for compat reasons we do not.
            //if (HasCertificateTableDirectory)
            //{
            //    specialHashingBlocks.Add(new DataBlock(DataBlockHashing.Hash, "CertificateTableDirectory", CertificateTableDirectoryOffset, CertificateTableDirectorySize));
            //}

            return HashingHelpers.SortAndJoinIntersectingHashingBlocks(hashingBlocks);
        }
#endregion

#region Checksum
        public void EraseChecksum()
        {
            EraseChecksum(_peStream);
        }

        private void EraseChecksum(Stream writablePEStream)
        {
            ExceptionsHelper.ThrowIfStreamNotWritable(writablePEStream);

            AssemblyMetadataExtractor dataExtractor = _dataExtractor.Value;

            // 0-initialized byte array
            byte[] newChecksum = new byte[dataExtractor.ChecksumSize];
            writablePEStream.Seek(dataExtractor.ChecksumOffset, SeekOrigin.Begin);
            writablePEStream.Write(newChecksum, 0, newChecksum.Length);
        }
#endregion

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            AssemblyMetadataExtractor dataExtractor = _dataExtractor.Value;

            sb.AppendLine("Signature directory size: {0}", dataExtractor.StrongNameSignatureDirectorySize);
            sb.AppendLine("AssemblyDefinition hash algorithm: {0}", AssemblyDefinitionPublicKeyBlob.HashAlgorithm);
            sb.AppendLine("Identity public key: {0}", IdentityPublicKeyBlob.ToString());
            sb.AppendLine("Identity public key: {0}", IdentityPublicKeyBlob.PublicKey.ToString());
            if (HasUniqueSignatureAndIdentityPublicKeyBlobs)
            {
                sb.AppendLine("Signature public key: {0}", SignaturePublicKeyBlob.ToString());
                sb.AppendLine("Signature public key: {0}", SignaturePublicKeyBlob.PublicKey.ToString());
                sb.AppendLine("Counter signature: {0}", CounterSignature.ToHex());
                sb.AppendLine("AssemblySignatureKeyAttribute hash algorithm: {0}", AssemblySignatureKeyAttributePublicKeyBlob.HashAlgorithm);

            }
            byte[] hash = ComputeHash();

            sb.AppendLine("Computed hash size: {0}", hash.Length);
            sb.AppendLine("Computed hash: {0}", hash.ToHex());

            foreach (var block in _hashingBlocks.Value)
            {
                sb.AppendLine(block.ToString());
            }

            sb.AppendLine("Number of sections = {0}", dataExtractor.NumberOfSections);
            sb.AppendLine("SectionsHeadersEndOffset = {0}", dataExtractor.SectionsHeadersEndOffset);
            foreach (SectionInfo section in dataExtractor.SectionsInfo)
            {
                sb.AppendLine(section.ToString());
            }

            return sb.ToString();
        }
    }
}
