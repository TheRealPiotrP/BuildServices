using System;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;

namespace MyGetConnector.Formatters
{
    public class VendorSpecificJsonMediaTypeFormatter : JsonMediaTypeFormatter
    {
        private readonly Type _resourceType;

        public VendorSpecificJsonMediaTypeFormatter(Type resourceType,
            MediaTypeHeaderValue vendorMediaType)
        {
            _resourceType = resourceType;

            SupportedMediaTypes.Clear();
            SupportedMediaTypes.Add(vendorMediaType); 
        }

        public override bool CanReadType(Type type)
        {
            return _resourceType == type;
        }

        public override bool CanWriteType(Type type)
        {
            return _resourceType == type;
        }
    }
}