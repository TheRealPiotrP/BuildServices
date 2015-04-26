using System;
using System.Net;
using FluentAssertions;
using Microsoft.Its.Recipes;
using Microsoft.Owin.MockService;
using Microsoft.Practices.Unity;
using Moq;
using MyGetConnector.Repositories;
using MyGetConnector.Tests.Extensions;
using Newtonsoft.Json;
using Owin;
using Xunit;

namespace MyGetConnector.Tests.MyGet
{
    public class PackageAddedSpecs
    {
        private IUnityContainer _container;

        public PackageAddedSpecs()
        {
            _container = TestUnityConfig.GetMockedContainer();
        }

        //[Fact]
        //public void When_it_is_a_PackageAdded_request_Then_it_fires_triggers_and_succeeds()
        //{
        //    var packageUri = Any.Uri();
            
        //    var triggerRepositoryMock = _container.GetMock<ITriggerRepository>();

        //    triggerRepositoryMock.Setup(t => t.FireTriggers(It.Is<Uri>(u => u.ToString() == packageUri.ToString())));

        //    using (var server = TestServer.Create(_container))
        //    {
        //        server.HttpClient.PostAsync("/api/MyGetWebhook/",
        //            Any.MyGet.WebHooks.PackageAddedPayload(packageUri).ToJsonStringContent())
        //            .Result
        //            .ShouldSucceed();
        //    }

        //    triggerRepositoryMock.Verify(t => t.FireTriggers(It.Is<Uri>(u => u.ToString() == packageUri.ToString())),
        //        Times.Once);
        //}
    }
}
