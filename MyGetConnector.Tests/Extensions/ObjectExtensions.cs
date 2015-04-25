using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Newtonsoft.Json
{
    public static class ObjectExtensions
    {
        public static string ToJsonString(this Object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        public static StringContent ToJsonStringContent(this Object obj)
        {
            return new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8,
                "application/json");
        }
    }
}
