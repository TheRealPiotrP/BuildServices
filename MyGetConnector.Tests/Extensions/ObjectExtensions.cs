using System;
using System.Net.Http;
using System.Text;

namespace Newtonsoft.Json
{
    public static class ObjectExtensions
    {
        public static string ToJsonString(this Object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static StringContent ToJsonStringContent(this Object obj, string contentType)
        {
            return new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, contentType);
        }

        public static StringContent ToJsonStringContent(this Object obj)
        {
            return obj.ToJsonStringContent("application/json");
        }
    }
}
