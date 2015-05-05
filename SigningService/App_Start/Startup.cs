using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Microsoft.Owin;
using Microsoft.Practices.Unity;
using Owin;
using SigningService;
using SigningService.Services.ExceptionHandling;
using Unity.WebApi;

[assembly: OwinStartup(typeof(Startup))]

namespace SigningService
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            Configuration(appBuilder, UnityConfig.GetConfiguredContainer());
        }

        public static void Configuration(IAppBuilder appBuilder, IUnityContainer unityContainer)
        {
            var config = new HttpConfiguration
            {
                DependencyResolver = new UnityDependencyResolver(unityContainer)
            };

            SwaggerConfig.Register(config);

            WebApiConfig.Register(config);

            config.Services.Add(typeof(IExceptionLogger), new TraceExceptionLogger());

            appBuilder.UseWebApi(config);
        }
    }
}
