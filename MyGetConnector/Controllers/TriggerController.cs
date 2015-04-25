using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Azure.AppService.ApiApps.Service;
using MyGetConnector.Repositories;

namespace MyGetConnector.Controllers
{
    public class TriggerController : ApiController
    {
        private readonly ITriggerRepository _triggerRepository;

        public TriggerController(ITriggerRepository triggerRepository)
        {
            _triggerRepository = triggerRepository;
        }

        public HttpResponseMessage Put(string triggerId,
            [FromBody] TriggerInput<string, string> triggerInput)
        {
            ClientTriggerCallback callback;

            try
            {
                callback = triggerInput.GetCallback();
            }
            catch (Exception)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            _triggerRepository.RegisterTrigger(triggerId, triggerInput);

            return Request.PushTriggerRegistered(callback);
        }
    }
}
