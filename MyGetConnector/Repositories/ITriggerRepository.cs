using System;
using System.Collections.Generic;
using Microsoft.Azure.AppService.ApiApps.Service;
using MyGetConnector.Models;

namespace MyGetConnector.Repositories
{
    public interface ITriggerRepository
    {
        void RegisterTrigger(string triggerId, TriggerInput<string, TriggerBody> triggerInput);

        IList<ClientTriggerCallback<TriggerBody>> GetTriggerCallbacks();
    }
}