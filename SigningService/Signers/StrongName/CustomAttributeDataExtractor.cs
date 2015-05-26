using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace SigningService.Signers.StrongName
{
    internal static class CustomAttributeDataExtractor
    {
        /// <summary>
        /// Gets name of the class of the custom attribute
        /// </summary>
        public static string GetCustomAttributeClassName(MetadataReader reader, CustomAttribute attribute)
        {
            switch (attribute.Constructor.Kind)
            {
                case HandleKind.MemberReference:
                {
                    MemberReferenceHandle memberRefHandle = (MemberReferenceHandle)attribute.Constructor;
                    MemberReference memberRef = reader.GetMemberReference(memberRefHandle);
                    return GetTypeName(reader, memberRef.Parent);
                }
                default:
                {
                    throw new NotImplementedException(string.Format("HandleKind = {0} not implemented.", attribute.Constructor.Kind));
                }
            }
        }

        /// <summary>
        /// Gets the fixed (required) string arguments of a custom attribute.
        /// Only attributes that have only fixed string arguments.
        /// </summary>
        public static List<string> GetFixedStringArguments(MetadataReader reader, CustomAttribute attribute)
        {
            BlobHandle signatureBlob = GetMethodSignature(reader, attribute.Constructor);
            BlobReader signatureReader = reader.GetBlobReader(signatureBlob);
            var valueReader = reader.GetBlobReader(attribute.Value);

            var prolog = valueReader.ReadUInt16();
            if (prolog != 1)
            {
                ExceptionsHelper.ThrowBadImageFormatException("Invalid custom attribute prolog.");
            }

            var header = signatureReader.ReadSignatureHeader();
            if (header.Kind != SignatureKind.Method || header.IsGeneric)
            {
                ExceptionsHelper.ThrowBadImageFormatException("Invalid custom attribute constructor signature.");
            }

            int parameterCount;
            if (!signatureReader.TryReadCompressedInteger(out parameterCount))
            {
                ExceptionsHelper.ThrowBadImageFormatException("Invalid custom attribute constructor signature.");
            }

            var returnType = signatureReader.ReadSignatureTypeCode();
            if (returnType != SignatureTypeCode.Void)
            {
                ExceptionsHelper.ThrowBadImageFormatException("Invalid custom attribute constructor signature.");
            }

           var strings = new List<string>();

            for (int i = 0; i < parameterCount; i++)
            {
                var signatureTypeCode = signatureReader.ReadSignatureTypeCode();
                switch (signatureTypeCode)
                {
                    case SignatureTypeCode.String:
                    {
                        strings.Add(valueReader.ReadSerializedString());
                        break;
                    }
                    default:
                    {
                        ExceptionsHelper.ThrowArgumentOutOfRange("customAttribute");
                        break;
                    }
                }
            }

            return strings;
        }

        private static BlobHandle GetMethodSignature(MetadataReader reader, Handle method)
        {
            switch (method.Kind)
            {
                case HandleKind.MemberReference:
                {
                    MemberReferenceHandle memberRefHandle = (MemberReferenceHandle)method;
                    MemberReference memberRef = reader.GetMemberReference(memberRefHandle);
                    return memberRef.Signature;
                }
                default:
                {
                    throw new NotImplementedException(string.Format("HandleKind = {0} not implemented.", method.Kind));
                }
            }
        }

        private static string GetTypeName(MetadataReader reader, Handle handle)
        {
            switch (handle.Kind)
            {
                case HandleKind.TypeReference:
                {
                    TypeReferenceHandle typeRefHandle = (TypeReferenceHandle)handle;
                    TypeReference typeRef = reader.GetTypeReference(typeRefHandle);
                    string nameSpace = reader.GetString(typeRef.Namespace);
                    string name = reader.GetString(typeRef.Name);
                    return nameSpace + "." + name;
                }
                default:
                {
                    throw new NotImplementedException(string.Format("HandleKind = {0} not implemented.", handle.Kind));
                }
            }
        }
    }
}