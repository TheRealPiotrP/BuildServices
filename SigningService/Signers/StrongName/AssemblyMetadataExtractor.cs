using SigningService.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace SigningService.Signers.StrongName
{
    /// <summary>
    /// Extracts all metadata from assembly needed to strong name sign
    /// </summary>
    internal class AssemblyMetadataExtractor
    {
        public AssemblyMetadataExtractor(Stream peStream)
        {
            _peStream = peStream;

            // Assuming peImage begins on offset 0
            peStream.Seek(0, SeekOrigin.Begin);

            ExtractAssemblyMetadata();
        }

        private Stream _peStream;

        public int CorFlagsOffset { get; private set; }
        public int CorFlagsSize { get { return 4; /*sizeof(UInt32)*/ } }
        public CorFlags CorFlagsValue { get; private set; }
        public bool HasStrongNameSignedFlag { get { return CorFlagsValue.HasFlag(CorFlags.StrongNameSigned); } }

        public int ChecksumOffset { get; private set; }
        public int ChecksumSize { get { return 4; /*sizeof(UInt32)*/ } }

        public int StrongNameSignatureDirectoryHeaderOffset { get; private set; }
        public int StrongNameSignatureDirectoryHeaderSize { get { return 8; /*sizeof(RVA) + sizeof(Size) = sizeof(UInt32) + sizeof(UInt32)*/ } }

        public bool HasStrongNameSignatureDirectory { get; private set; }
        public int StrongNameSignatureDirectoryOffset { get; private set; }
        public int StrongNameSignatureDirectorySize { get; private set; }

        public int CertificateTableDirectoryHeaderOffset { get; private set; }
        public int CertificateTableDirectoryHeaderSize { get { return 8; /*sizeof(RVA) + sizeof(Size) = sizeof(UInt32) + sizeof(UInt32)*/ } }

        public bool HasCertificateTableDirectory { get; private set; }
        public int CertificateTableDirectoryOffset { get; private set; }
        public int CertificateTableDirectorySize { get; private set; }

        public int SectionsHeadersEndOffset { get; private set; }
        public int NumberOfSections { get; private set; }
        public List<SectionInfo> SectionsInfo { get; private set; }
        public int PaddingBetweenTheSectionHeadersAndSectionsOffset { get { return SectionsHeadersEndOffset; } }

        public PublicKeyBlob AssemblyDefinitionPublicKeyBlob { get; private set; }
        public PublicKeyBlob AssemblySignatureKeyAttributePublicKeyBlob { get; private set; }
        public byte[] AssemblySignatureKeyAttributeCounterSignatureBlob { get; private set; }

        /// <summary>
        /// Extracts metadata from assembly
        /// </summary>
        private void ExtractAssemblyMetadata()
        {
            using (PEReader peReader = new PEReader(_peStream, PEStreamOptions.LeaveOpen | PEStreamOptions.PrefetchEntireImage | PEStreamOptions.PrefetchMetadata))
            {
                CorFlagsOffset = peReader.PEHeaders.CorHeader.FlagsOffset;
                ChecksumOffset = peReader.PEHeaders.PEHeader.CheckSumOffset;
                CorFlagsValue = peReader.PEHeaders.CorHeader.Flags;

                StrongNameSignatureDirectoryHeaderOffset = peReader.PEHeaders.CorHeader.StrongNameSignatureDirectory.HeaderOffset;
                int strongNameSignatureDirectoryOffset;
                HasStrongNameSignatureDirectory = peReader.PEHeaders.TryGetDirectoryOffset(peReader.PEHeaders.CorHeader.StrongNameSignatureDirectory, out strongNameSignatureDirectoryOffset);
                if (HasStrongNameSignatureDirectory)
                {
                    StrongNameSignatureDirectoryOffset = strongNameSignatureDirectoryOffset;
                    StrongNameSignatureDirectorySize = peReader.PEHeaders.CorHeader.StrongNameSignatureDirectory.Size;
                }

                CertificateTableDirectoryHeaderOffset = peReader.PEHeaders.PEHeader.CertificateTableDirectory.HeaderOffset;
                int certificateTableDirectoryOffset;
                HasCertificateTableDirectory = peReader.PEHeaders.TryGetDirectoryOffset(peReader.PEHeaders.PEHeader.CertificateTableDirectory, out certificateTableDirectoryOffset);
                if (HasCertificateTableDirectory)
                {
                    CertificateTableDirectoryOffset = certificateTableDirectoryOffset;
                    CertificateTableDirectorySize = peReader.PEHeaders.PEHeader.CertificateTableDirectory.Size;
                }

                SectionsHeadersEndOffset = peReader.PEHeaders.SectionsHeadersEndOffset;
                NumberOfSections = peReader.PEHeaders.CoffHeader.NumberOfSections;
                if (NumberOfSections > 0)
                {
                    List<SectionInfo> sections = new List<SectionInfo>(NumberOfSections);
                    for (int i = 0; i < NumberOfSections; i++)
                    {
                        SectionInfo si = new SectionInfo();
                        si.Name = peReader.PEHeaders.SectionHeaders[i].Name;
                        si.Offset = peReader.PEHeaders.SectionHeaders[i].PointerToRawData;
                        si.Size = peReader.PEHeaders.SectionHeaders[i].SizeOfRawData;
                        sections.Add(si);
                    }
                    SectionsInfo = sections;
                }
                else
                {
                    ExceptionsHelper.ThrowPEImageHasNoSections();
                    return;
                }

                MetadataReader mr = peReader.GetMetadataReader();
                AssemblyDefinition assemblyDef = mr.GetAssemblyDefinition();
                AssemblyDefinitionPublicKeyBlob = new PublicKeyBlob(mr.GetBlobBytes(assemblyDef.PublicKey));

                foreach (CustomAttributeHandle cah in mr.GetCustomAttributes(Handle.AssemblyDefinition))
                {
                    CustomAttribute ca = mr.GetCustomAttribute(cah);
                    string className = CustomAttributeDataExtractor.GetCustomAttributeClassName(mr, ca);
                    if (className == "System.Reflection.AssemblySignatureKeyAttribute")
                    {
                        List<string> args = CustomAttributeDataExtractor.GetFixedStringArguments(mr, ca);
                        if (args.Count == 2)
                        {
                            AssemblySignatureKeyAttributePublicKeyBlob = new PublicKeyBlob(args[0]);
                            byte[] counterSignature = args[1].FromHexToByteArray();
                            counterSignature.ReverseInplace();
                            AssemblySignatureKeyAttributeCounterSignatureBlob = counterSignature;
                        }
                    }
                }
            }
        }
    }
}
