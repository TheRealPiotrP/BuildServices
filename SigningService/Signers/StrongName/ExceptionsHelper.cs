using System;
using System.IO;

namespace SigningService.Signers.StrongName
{
    internal static class ExceptionsHelper
    {
#region Conditional
        public static void ThrowIfStreamNotWritable(Stream s)
        {
            if (!s.CanWrite)
            {
                throw new StrongNameSignerException("Stream is not writable!");
            }
        }
#endregion

        public static void ThrowBadImageFormatException(string message = null)
        {
            string additionalInfo = "";
            if (!string.IsNullOrWhiteSpace(message))
            {
                additionalInfo = string.Format(" {1}", message);
            }
            throw new BadImageFormatException(string.Format("Bad format exception!{0}", additionalInfo));
        }

        public static void ThrowArgumentOutOfRange(string paramName)
        {
            throw new StrongNameSignerException("Argument {0} out of range!", paramName);
        }

        public static void ThrowAssemblyAlreadySigned()
        {
            throw new StrongNameSignerException("Assembly is already strong name signed!");
        }

        public static void ThrowNoStrongNameSignature()
        {
            throw new StrongNameSignerException("Assembly is not strong name signed!");
        }

        public static void ThrowNoStrongNameSignatureDirectory()
        {
            throw new StrongNameSignerException("Assembly does not have strong name signature directory!");
        }

        public static void ThrowStrongNameSignatureDirectorySizeIsDifferentThanProvidedSignature(long strongNameSignatureDirectorySize, long signatureSize)
        {
            throw new StrongNameSignerException("Assembly has different strong name signature directory size than provided signature! Strong name signature directory size: {0}. Size of provided signature: {1}", strongNameSignatureDirectorySize, signatureSize);
        }

        public static void ThrowAssemblyNotHashable()
        {
            throw new StrongNameSignerException("Assembly is not hashable!");
        }

        public static void ThrowPEImageHasNoSections()
        {
            ThrowBadImageFormatException("PE Image has no sections!");
        }

        public static void ThrowUnexpectedEndOfStream(long position)
        {
            string message = string.Format("Unexpected end of stream on position {0}!", position);
            ThrowBadImageFormatException(message);
        }
    }
}
