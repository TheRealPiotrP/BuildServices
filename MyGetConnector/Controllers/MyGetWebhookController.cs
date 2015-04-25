using System.Web.Http;
using MyGetConnector.Agents;
using MyGetConnector.Demultiplexers;
using Signature.Web.Models;

namespace MyGetConnector.Controllers
{
    public class MyGetWebhookController : ApiController
    {
        private readonly MyGetWebhookDemultiplexer _myGetWebhookDemultiplexer;

        public MyGetWebhookController(MyGetWebhookDemultiplexer myGetWebhookDemultiplexer)
        {
            _myGetWebhookDemultiplexer = myGetWebhookDemultiplexer;
        }

        public void Post([FromBody]WebHookEvent value)
        {
            _myGetWebhookDemultiplexer.Demultiplex(value);
        }
    }
}
