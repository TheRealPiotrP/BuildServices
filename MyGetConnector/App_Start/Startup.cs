using System.Web.Http;
using Microsoft.Owin;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.WebApi;
using MyGetConnector;
using MyGetConnector.App_Start;
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

            appBuilder.UseWebApi(config);
        }
    }
}
