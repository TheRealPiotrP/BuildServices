using System.Net.Http;
using Microsoft.Its.Recipes;

namespace MyGetConnector.Tests.Extensions
{
    public static class HttpClientExtensions
    {
        public static HttpClient AsMyGetTriggerSource(this HttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.Add("User-agent", "MyGet Web Hook/1.0");
            httpClient.DefaultRequestHeaders.Add("X-MyGet-EventIdentifier", Any.Guid().ToString());

            return httpClient;;
        }
    }
}
