using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MyGetConnector.Repositories;
using Signature.Web.Models;

namespace MyGetConnector.Demultiplexers
{
    public class MyGetWebhookDemultiplexer
    {
        private readonly ITriggerRepository _triggerRepository;

        public MyGetWebhookDemultiplexer(ITriggerRepository triggerRepository)
        {
            _triggerRepository = triggerRepository;
        }

        public void Demultiplex(WebHookEvent webHookEvent)   
        {
            switch (webHookEvent.PayloadType)
            {
                case "PackageAddedWebHookEventPayloadV1":
                    _triggerRepository.FireTriggers(new Uri(webHookEvent.Payload.PackageDownloadUrl));
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
