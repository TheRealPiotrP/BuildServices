using System;
using Microsoft.Practices.Unity;

namespace MyGetConnector.Tests
{
    public static class TestServer
    {
        public static Microsoft.Owin.Testing.TestServer Create(IUnityContainer container)
        {
            return Microsoft.Owin.Testing.TestServer.Create(builder => Startup.Configuration(builder, container));
        }
    }
}
