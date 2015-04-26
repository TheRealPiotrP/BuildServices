using System.Threading.Tasks;
using Signature.Web.Models;

namespace MyGetConnector.Demultiplexers
{
    public interface IMyGetWebhookDemultiplexer
    {
        Task Demultiplex(WebHookEvent webHookEvent);
    }
}