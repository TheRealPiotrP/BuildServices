using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.ExceptionHandling;
using Microsoft.Owin;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.WebApi;
using MyGetConnector;
using MyGetConnector.App_Start;
using MyGetConnector.Services.ExceptionHandling;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace MyGetConnector
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

            FormatterConfig.Register(config);

            config.Services.Add(typeof(IExceptionLogger), new TraceExceptionLogger());

            appBuilder.UseWebApi(config);
        }
    }
}
