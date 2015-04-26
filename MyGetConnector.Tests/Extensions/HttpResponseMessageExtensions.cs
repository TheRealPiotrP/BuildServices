using System.Net;
using System.Net.Http;

namespace FluentAssertions
{
    public static class HttpResponseMessageExtensions
    {
        public static void ShouldSucceed(this HttpResponseMessage httpResponseMessage)
        {
            httpResponseMessage.IsSuccessStatusCode.Should().BeTrue();
        }
        public static void ShouldFail(this HttpResponseMessage httpResponseMessage)
        {
            httpResponseMessage.IsSuccessStatusCode.Should().BeFalse();
        }

        public static void ShouldSucceedWith(this HttpResponseMessage httpResponseMessage, HttpStatusCode httpStatusCode)
        {
            httpResponseMessage.ShouldSucceed();

            httpResponseMessage.StatusCode.Should().Be(httpStatusCode);
        }

        public static void ShouldFailWith(this HttpResponseMessage httpResponseMessage, HttpStatusCode httpStatusCode)
        {
            httpResponseMessage.ShouldFail();

            httpResponseMessage.StatusCode.Should().Be(httpStatusCode);
        }
    }
}
