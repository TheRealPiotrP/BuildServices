using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using MyGetConnector.Agents;
using Signature.Web.Models;

namespace MyGetConnector.Demultiplexers
{
    public class MyGetWebhookDemultiplexer : IMyGetWebhookDemultiplexer
    {
        private readonly IAddPackageAgent _addPackageAgent;

        public MyGetWebhookDemultiplexer(IAddPackageAgent addPackageAgent)
        {
            _addPackageAgent = addPackageAgent;
        }

        public async Task Demultiplex(WebHookEvent webHookEvent)   
        {
            switch (webHookEvent.PayloadType)
            {
                case "PackageAddedWebHookEventPayloadV1":
                    await _addPackageAgent.AddPackage(new Uri(webHookEvent.Payload.PackageDownloadUrl));
                    break;
                default:
                    throw new HttpResponseException(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        ReasonPhrase = "Unknown PayloadType",
                    });
            }
        }
    }
}
