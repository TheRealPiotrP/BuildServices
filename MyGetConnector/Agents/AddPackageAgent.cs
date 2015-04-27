using System;
using System.Net.Http;
using System.Threading.Tasks;
using Its.Log.Instrumentation;
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
                catch (HttpRequestException e)
                {
                    // Server Unavailable
                    Log.Write(() => e);
                }
                catch (InvalidOperationException e)
                {
                    // Bad Request
                    Log.Write(() => e);
                }
            }
        }
    }
}
