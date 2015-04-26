using System.Threading.Tasks;
using System.Web.Http;
using MyGetConnector.Agents;
using MyGetConnector.Demultiplexers;
using Signature.Web.Models;

namespace MyGetConnector.Controllers
{
    public class MyGetWebhookController : ApiController
    {
        private readonly IMyGetWebhookDemultiplexer _myGetWebhookDemultiplexer;

        public MyGetWebhookController(IMyGetWebhookDemultiplexer myGetWebhookDemultiplexer)
        {
            _myGetWebhookDemultiplexer = myGetWebhookDemultiplexer;
        }

        public async Task Post([FromBody]WebHookEvent value)
        {
            await _myGetWebhookDemultiplexer.Demultiplex(value);
        }
    }
}
