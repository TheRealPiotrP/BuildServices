using System;
using System.IO.Packaging;
using System.Net.Http;
using FluentAssertions;
using Microsoft.Its.Recipes;
using Microsoft.Owin.MockService;
using SigningService.Agents;
using SigningService.Tests.Properties;
using Xunit;

namespace SigningService.Tests
{
    public class PackageAgentSpecs
    {
        [Fact]
        public async void When_given_a_valid_package_Url_it_returns_the_package()
        {
            var packageAgent = new PackageAgent();

            var packageName = Any.Word() + ".nupkg";

            Package package;

            using (var mockService = new MockService()
                .OnRequest(r => r.Path.ToString() == "/" + packageName)
                .RespondWith(r => r.WriteAsync(Resources.Microsoft_Bcl_1_1_8)))
            {
                var packageUri = new Uri(mockService.GetBaseAddress() + packageName);

                package = await packageAgent.GetPackage(packageUri);
            }

            package.Should().NotBeNull("Because it should be retrieved");

            package.GetParts()
                .Should()
                .HaveCount(92, "Because that is how many package parts are in the Microsoft.Bcl.1.1.8 package");
        }

        [Fact]
        public void When_given_a_null_packageUrl_it_throws_an_exception_with_a_useful_message()
        {
            var packageAgent = new PackageAgent();

            Action getPackageAction = () => packageAgent.GetPackage(null).Wait();

            getPackageAction
                .ShouldThrow<ArgumentNullException>("Because the packageUrl cannot be null")
                .WithMessage("Value cannot be null.\r\nParameter name: packageUri");
        }

        [Fact]
        public void When_given_an_inaccesible_packageUrl_it_throws_an_exception_with_a_useful_message()
        {
            var packageAgent = new PackageAgent();

            var packageName = Any.Word() + ".nupkg";

            using (var mockService = new MockService()
                .OnRequest(r => r.Path.ToString() == "/" + packageName)
                .RespondWith(r => r.StatusCode = 403))
            {
                var packageUri = new Uri(mockService.GetBaseAddress() + packageName);

                Action getPackageAction = () => packageAgent.GetPackage(packageUri).Wait();

                getPackageAction
                    .ShouldThrow<HttpRequestException>("Because the packageUrl cannot be null")
                    .WithMessage("Response status code does not indicate success: 403 (Forbidden).");
            }
        }
    }
}
