using System.Net.Http.Headers;
using System.Web.Http;
using MyGetConnector.Formatters;
using Signature.Web.Models;

namespace MyGetConnector
{
    public static class FormatterConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Formatters.Add(new VendorSpecificJsonMediaTypeFormatter(typeof(WebHookEvent),
                new MediaTypeHeaderValue("application/vnd.myget.webhooks.v1+json")));
        }
    }
}