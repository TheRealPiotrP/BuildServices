using System;
using Microsoft.Azure.AppService.ApiApps.Service;

namespace MyGetConnector.Repositories
{
    public interface ITriggerRepository
    {
        void RegisterTrigger(string triggerId, TriggerInput<string, string> triggerInput);
        void FireTriggers(Uri packageUri);
    }
}