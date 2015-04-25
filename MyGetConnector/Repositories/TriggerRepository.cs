using System;
using System.Collections.Generic;
using Microsoft.Azure.AppService.ApiApps.Service;

namespace MyGetConnector.Repositories
{
    public class TriggerRepository : ITriggerRepository
    {
        private readonly IDictionary<string, TriggerInput<string, string>> _store;

        public TriggerRepository()
        {
            _store = new Dictionary<string, TriggerInput<string, string>>();
        }

        // Register a push trigger.
        public void RegisterTrigger(string triggerId, TriggerInput<string, string> triggerInput)
        {
            _store.Add(triggerId, triggerInput);
        }

        // Fire the assoicated push trigger when some file is changed.
        public void FireTriggers(Uri packageUri)
        {
            foreach (var input in _store.Values)
            {
                input.GetCallback().InvokeAsync(Runtime.FromAppSettings(), packageUri);
            }
        }
    }
}