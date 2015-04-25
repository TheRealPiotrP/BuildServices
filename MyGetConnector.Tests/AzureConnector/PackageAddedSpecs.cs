using System;
using System.Net;
using FluentAssertions;
using Microsoft.Azure.AppService.ApiApps.Service;
using Microsoft.Its.Recipes;
using Microsoft.Owin.Testing;
using Microsoft.Practices.Unity;
using Moq;
using MyGetConnector.Repositories;
using MyGetConnector.Tests.Extensions;
using Newtonsoft.Json;
using Xunit;

namespace MyGetConnector.Tests.AzureConnector
{
    public class TriggerRegistrationSpecs
    {
        private IUnityContainer container;

        public TriggerRegistrationSpecs()
        {
            container = TestUnityConfig.GetMockedContainer();
        }

        [Fact]
        public void When_trigger_request_is_valid_Then_service_adds_trigger_to_repository_and_returns_200()
        {
            var callbackUrl = Any.Uri().ToString();

            var triggerId = Any.String(1);

            var triggerRepositoryMock = container.GetMock<ITriggerRepository>();
                
            triggerRepositoryMock
                .Setup(t => t.RegisterTrigger(It.IsAny<string>(), It.IsAny<TriggerInput<string, string>>()));

            using (var server = TestServer.Create(container))
            {
                server.HttpClient.PutAsync(String.Format("/api/Trigger?triggerId={0}", triggerId),
                    Any.Azure.AppServiceTrigger.MyGetConnectorTriggerBody(callbackUrl).ToJsonStringContent())
                    .Result
                    .ShouldSucceedWith(HttpStatusCode.OK);
            }

            triggerRepositoryMock
                .Verify(r => r.RegisterTrigger(
                        It.Is<string>(s => s == triggerId),
                        It.Is<TriggerInput<string, string>>(t => t.GetCallback().CallbackUri.ToString() == callbackUrl)), Times.Once);
        }

        [Fact]
        public void When_trigger_request_is_missing_triggerId_Then_service_returns_404()
        {
            var triggerRepositoryMock = container.GetMock<ITriggerRepository>();

            using (var server = TestServer.Create(container))
            {
                // Execute test against the web API.
                server.HttpClient.PutAsync("/api/Trigger",
                    Any.Azure.AppServiceTrigger.MyGetConnectorTriggerBody().ToJsonStringContent())
                    .Result
                    .ShouldFailWith(HttpStatusCode.NotFound);
            }

            triggerRepositoryMock
                .Verify(r => r.RegisterTrigger(
                        It.IsAny<string>(),
                        It.IsAny<TriggerInput<string, string>>()), Times.Never);
        }

        [Fact]
        public void When_trigger_request_has_invalid_callbackUrl_Then_service_returns_400()
        {
            var triggerRepositoryMock = container.GetMock<ITriggerRepository>();

            using (var server = TestServer.Create(container))
            {
                // Execute test against the web API.
                server.HttpClient.PutAsync(String.Format("/api/Trigger?triggerId={0}", Any.String(1)),
                    Any.Azure.AppServiceTrigger.MyGetConnectorTriggerBody(Any.String()).ToJsonStringContent())
                    .Result
                    .ShouldFailWith(HttpStatusCode.BadRequest);
            }

            triggerRepositoryMock
                .Verify(r => r.RegisterTrigger(
                        It.IsAny<string>(),
                        It.IsAny<TriggerInput<string, string>>()), Times.Never);
        }
    }
}
