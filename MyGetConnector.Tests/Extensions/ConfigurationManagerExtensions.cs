using System.Configuration;
using Microsoft.Its.Recipes;

namespace MyGetConnector.Tests.Extensions
{
    public static class ConfigurationManagerExtensions
    {
        public static void AddFakeAppServiceRuntimeSettings(string runtimeUrl = null)
        {
            ConfigurationManager.AppSettings["EMA_RuntimeUrl"] = runtimeUrl ?? Any.Uri().ToString();
            ConfigurationManager.AppSettings["EMA_MicroserviceId"] = Any.Guid().ToString();
            ConfigurationManager.AppSettings["EMA_Secret"] = Any.String(32, 32);
        }
    }
}
