using System;
using System.Web.Http;
using SigningService.Agents;
using SigningService.Models;

namespace SigningService.Controllers
{
    public class SignController : ApiController
    {
        private readonly ISignAgent _signAgent;

        public SignController(ISignAgent signAgent)
        {
            _signAgent = signAgent;
        }

        // POST api/values
        public async void Post([FromBody]Package package)
        {
            var packageUri = new Uri(package.PackageUrl);

            await _signAgent.SignPackage(packageUri);
        }
    }
}
