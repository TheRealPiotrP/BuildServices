using System;
using System.Configuration;
using System.Linq;
using FluentAssertions;
using Microsoft.Its.Recipes;
using Microsoft.Owin.MockService;
using Moq;
using MyGetConnector.Repositories;
using MyGetConnector.Tests.Extensions;
using Newtonsoft.Json;
using Xunit;

namespace MyGetConnector.Tests.AcceptanceTests
{
    public class PackageAddedSpecs
    {
        [Fact]
        public void When_no_trigger_callbacks_are_registered_Then_the_request_succeeds()
        {
            var packageUri = Any.Uri();

            using (var server = Microsoft.Owin.Testing.TestServer.Create<Startup>())
            {
                server.HttpClient.PostAsync("/api/MyGetWebhook/",
                    Any.MyGet.WebHooks.PackageAddedPayload(packageUri).ToJsonStringContent())
                    .Result
                    .ShouldSucceed();
            }
        }

        [Fact]
        public void When_one_trigger_callback_is_registered_Then_the_callback_is_notified_the_request_succeeds()
        {
            ConfigurationManagerExtensions.AddFakeAppServiceRuntimeSettings();

            var packageUri = Any.Uri();

            var callbackPath1 = Any.Uri().AbsolutePath;

            using (var mockService = new MockService()
                .OnRequest(r => r.Path.ToString() == callbackPath1)
                .RespondWith(r => r.StatusCode = 200))
            {
                var callbackUri = mockService.GetBaseAddress() + callbackPath1;

                using (var server = Microsoft.Owin.Testing.TestServer.Create<Startup>())
                {
                    server.HttpClient.PutAsync(String.Format("/api/Trigger?triggerId={0}", Any.Guid()),
                        Any.Azure.AppServiceTrigger.MyGetConnectorTriggerBody(callbackUri)
                            .ToJsonStringContent())
                        .Wait();

                    server.HttpClient.PostAsync("/api/MyGetWebhook/",
                        Any.MyGet.WebHooks.PackageAddedPayload(packageUri).ToJsonStringContent())
                        .Result
                        .ShouldSucceed();
                }
            }
        }
        
        [Fact]
        public void When_multiple_trigger_callbacks_are_registered_Then_the_callbacks_are_notified_the_request_succeeds()
        {
            ConfigurationManagerExtensions.AddFakeAppServiceRuntimeSettings();

            var packageUri = Any.Uri();

            var callbackPaths = Any.Sequence(x => Any.Uri().AbsolutePath).ToList();

            using (var mockService = new MockService())
            {
                using (var server = Microsoft.Owin.Testing.TestServer.Create<Startup>())
                {
                    foreach (var callbackPath in callbackPaths)
                    {
                        var path = callbackPath;

                        mockService
                            .OnRequest(r => r.Path.ToString() == path)
                            .RespondWith(r => r.StatusCode = 200);

                        var callbackUri = mockService.GetBaseAddress() + path;

                        server.HttpClient.PutAsync(String.Format("/api/Trigger?triggerId={0}", Any.Guid()),
                            Any.Azure.AppServiceTrigger.MyGetConnectorTriggerBody(callbackUri)
                                .ToJsonStringContent())
                            .Wait();
                    }

                    server.HttpClient.PostAsync("/api/MyGetWebhook/",
                        Any.MyGet.WebHooks.PackageAddedPayload(packageUri).ToJsonStringContent())
                        .Result
                        .ShouldSucceed();
                }
            }
        }

        [Fact]
        public void When_one_trigger_callback_is_registered_but_target_is_not_available_Then_the_request_succeeds()
        {
            var packageUri = Any.Uri();

            ConfigurationManagerExtensions.AddFakeAppServiceRuntimeSettings();

            using (var server = Microsoft.Owin.Testing.TestServer.Create<Startup>())
            {
                server.HttpClient.PutAsync(String.Format("/api/Trigger?triggerId={0}", Any.Guid()),
                    Any.Azure.AppServiceTrigger.MyGetConnectorTriggerBody(Any.Uri().ToString()).ToJsonStringContent())
                    .Wait();

                server.HttpClient.PostAsync("/api/MyGetWebhook/",
                    Any.MyGet.WebHooks.PackageAddedPayload(packageUri).ToJsonStringContent())
                    .Result
                    .ShouldSucceed();
            }
        }

        [Fact]
        public void When_multiple_trigger_callbacks_are_registered_and_one_fails_Then_the_remaining_callbacks_are_notified_the_request_succeeds()
        {
            ConfigurationManagerExtensions.AddFakeAppServiceRuntimeSettings();

            var packageUri = Any.Uri();

            var callbackPaths = Any.Sequence(x => Any.Uri().AbsolutePath).ToList();

            using (var mockService = new MockService())
            {
                using (var server = Microsoft.Owin.Testing.TestServer.Create<Startup>())
                {
                    foreach (var callbackPath in callbackPaths)
                    {
                        var path = callbackPath;

                        if (Any.Bool())
                        {
                            mockService
                                .OnRequest(r => r.Path.ToString() == path)
                                .RespondWith(r => r.StatusCode = 200);
                        }

                        var callbackUri = mockService.GetBaseAddress() + path;

                        server.HttpClient.PutAsync(String.Format("/api/Trigger?triggerId={0}", Any.Guid()),
                            Any.Azure.AppServiceTrigger.MyGetConnectorTriggerBody(callbackUri)
                                .ToJsonStringContent())
                            .Wait();
                    }

                    server.HttpClient.PostAsync("/api/MyGetWebhook/",
                        Any.MyGet.WebHooks.PackageAddedPayload(packageUri).ToJsonStringContent())
                        .Result
                        .ShouldSucceed();
                }
            }
        }
    }
}
