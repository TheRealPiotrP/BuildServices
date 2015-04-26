using System;
using System.Threading.Tasks;
using Microsoft.Azure.AppService.ApiApps.Service;
using MyGetConnector.Models;
using MyGetConnector.Repositories;

namespace MyGetConnector.Agents
{
    public class AddPackageAgent : IAddPackageAgent
    {
        private readonly ITriggerRepository _triggerRepository;

        public AddPackageAgent(ITriggerRepository triggerRepository)
        {
            _triggerRepository = triggerRepository;
        }

        public async Task AddPackage(Uri packageUrl)
        {
            var callbacks = _triggerRepository.GetTriggerCallbacks();

            var triggerBody = new TriggerBody {PackageUrl = packageUrl.ToString()};

            foreach (var callback in callbacks)
            {
                try
                {
                    await callback.InvokeAsync(
                        Runtime.FromAppSettings(),
                        triggerBody);
                }
                catch (InvalidOperationException)
                {
                    // ignoring failures due to bad callback Url
                }
            }
        }
    }
}
