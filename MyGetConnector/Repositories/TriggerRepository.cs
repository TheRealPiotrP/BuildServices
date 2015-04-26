using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.AppService.ApiApps.Service;
using MyGetConnector.Models;

namespace MyGetConnector.Repositories
{
    public class TriggerRepository : ITriggerRepository
    {
        private readonly IDictionary<string, TriggerInput<string, TriggerBody>> _store =
            new Dictionary<string, TriggerInput<string, TriggerBody>>();

        // Register a push trigger.
        public void RegisterTrigger(string triggerId, TriggerInput<string, TriggerBody> triggerInput)
        {
            lock (_store)
            {
                _store.Add(triggerId, triggerInput);
            }
        }

        public IList<ClientTriggerCallback<TriggerBody>> GetTriggerCallbacks()
        {
            IList<ClientTriggerCallback<TriggerBody>> callbacks;

            lock (_store)
            {
                callbacks = _store.Values.Select(v => v.GetCallback()).ToList();
            }

            return callbacks;
        }
    }
}