using System.Net.Http;
using Microsoft.Practices.Unity;
using Moq;
using MyGetConnector.Repositories;

namespace MyGetConnector.Tests
{
    public static class TestUnityConfig
    {
        public static IUnityContainer GetMockedContainer()
        {
            var container = new UnityContainer();
            container.RegisterInstance(new HttpClient());
            container.RegisterInstance(typeof(ITriggerRepository), new Mock<ITriggerRepository>(MockBehavior.Strict).Object);

            return container;
        }
    }
}
