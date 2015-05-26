using SigningService.Extensions;
using SigningService.Models;
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace SigningService.Signers.StrongName
{
    // Public Key Blob should look as following:
    // 12 bytes header:
    //      - Signature Algorithm Id (4 bytes)
    //      - Hash Algorithm Id (4 bytes)
    //      - Public Key Struct Size (4 bytes)
    //
    // Public Key struct
    //      - Prefix (4 bytes) -> (1b - type, 1b - version, 2b - reserved)
    //      - Signature Algorithm Id (4 bytes)
    //      - "RSA1" (4 bytes)
    //      - Modulus size in bits (4 bytes)
    //      - Exponent (4 bytes)
    //      - Modulus bytes (var size)
    internal class PublicKeyBlob
    {
        private struct PublicKeyBlobLayout
        {
            public const byte TypePublicKeyBlob = 0x06;
            public const UInt32 Rsa1 = 0x31415352;
            public const int HeaderSize = 12;
            public const int StructSizeMinusModulusSize = 32;

            public PublicKeyBlobLayout(byte[] publicKeyBlob) : this()
            {
                using (MemoryStream ms = new MemoryStream(publicKeyBlob))
                {
                    BinaryReader binaryReader = new BinaryReader(ms);

                    AlgId = binaryReader.ReadUInt32();
                    HashAlgorithm = (AssemblyHashAlgorithm)binaryReader.ReadUInt32();
                    PublicKeyStructSize = binaryReader.ReadUInt32();

                    Type = binaryReader.ReadByte();
                    Version = binaryReader.ReadByte();
                    Reserved = binaryReader.ReadUInt16();
                    KeyAlg = binaryReader.ReadUInt32();
                    Magic = binaryReader.ReadUInt32();
                    ModulusBitLength = binaryReader.ReadInt32();
                    PublicExponent = binaryReader.ReadBytes(4);
                    Modulus = binaryReader.ReadBytes(ModulusBitLength / 8);
                    Modulus.ReverseInplace();
                }
            }

            // header
            public UInt32 AlgId;
            public AssemblyHashAlgorithm HashAlgorithm;
            public UInt32 PublicKeyStructSize;

#region PUBLICKEYBLOB https://msdn.microsoft.com/en-us/library/windows/desktop/aa375601%28v=vs.85%29.aspx
#region PUBLICKEYSTRUC https://msdn.microsoft.com/en-us/library/windows/desktop/aa387453%28v=vs.85%29.aspx
            // blob header
            public byte Type;
            public byte Version;
            public UInt16 Reserved;
            public UInt32 KeyAlg;
#endregion

#region RSAPUBKEY https://msdn.microsoft.com/en-us/library/windows/desktop/aa387685%28v=vs.85%29.aspx
            // RSA Public Key
            public UInt32 Magic;
            public Int32 ModulusBitLength;
            public byte[] PublicExponent;
#endregion

            public byte[] Modulus;
#endregion
        }

        private PublicKeyBlobLayout data;

        public PublicKeyBlob(byte[] publicKeyBlob)
        {
            data = new PublicKeyBlobLayout(publicKeyBlob);

            if (data.Type != PublicKeyBlobLayout.TypePublicKeyBlob)
            {
                ExceptionsHelper.ThrowBadImageFormatException("Expected public key blob!");
            }

            if (data.Magic != PublicKeyBlobLayout.Rsa1)
            {
                ExceptionsHelper.ThrowBadImageFormatException("Only RSA1 is supported!");
            }

            if ((PublicKeyBlobLayout.HeaderSize + data.PublicKeyStructSize != publicKeyBlob.Length)
                || (PublicKeyBlobLayout.StructSizeMinusModulusSize + data.Modulus.Length != publicKeyBlob.Length))
            {
                ExceptionsHelper.ThrowBadImageFormatException("Invalid size of public key blob!");
            }

            Blob = publicKeyBlob;
            PublicKey = new PublicKey(data.PublicExponent, data.Modulus);
            _publicKeyToken = new Lazy<string>(GetPublicKeyToken);
        }

        public PublicKeyBlob(string publicKeyBlobHex)
            : this(publicKeyBlobHex.FromHexToByteArray()) { }

        private Lazy<string> _publicKeyToken;
        public byte[] Blob { get; private set; }
        public PublicKey PublicKey { get; private set; }
        public string PublicKeyToken { get { return _publicKeyToken.Value; } }
        public AssemblyHashAlgorithm HashAlgorithm { get { return data.HashAlgorithm; } }

        private string GetPublicKeyToken()
        {
            byte[] ret = new byte[8];
            HashAlgorithm sha1 = SHA1.Create();

            sha1.TransformFinalBlock(Blob, 0, Blob.Length);

            for (int i = 0; i < 8; i++)
            {
                ret[i] = sha1.Hash[sha1.Hash.Length - i - 1];
            }
            
            return ret.ToHex();
        }

        /// <summary>
        /// Hashes the data, decrypts the hash and compares
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool VerifyData(byte[] data, byte[] signature)
        {
            //byte[] decryptedHash = PublicKey.Encrypt(encryptedHash);
            HashAlgorithm hashAlgorithm = HashingHelpers.CreateHashAlgorithm(HashAlgorithm);
            if (hashAlgorithm == null)
            {
                ExceptionsHelper.ThrowArgumentOutOfRange("HashAlgorithm");
            }

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            RSAParameters rsap = rsa.ExportParameters(false);
            // Trailing zeros do not change Exponent/Modulus value
            // but cause CryptoProvider to throw
            rsap.Exponent = RemoveTrailingZeros(PublicKey.Exponent);
            rsap.Modulus = RemoveTrailingZeros(PublicKey.Modulus);
            rsa.ImportParameters(rsap);

            return rsa.VerifyData(data, hashAlgorithm.GetType(), signature);
        }

        private static byte[] RemoveTrailingZeros(byte[] bytes)
        {
            int lastByte = bytes.Length - 1;
            for (; lastByte > 0; lastByte--)
            {
                if (bytes[lastByte] != 0)
                {
                    break;
                }
            }

            byte[] ret = new byte[lastByte + 1];
            Array.Copy(bytes, 0, ret, 0, ret.Length);

            return ret;
        }

        public override string ToString()
        {
            return Blob.ToHex();
        }
    }
}