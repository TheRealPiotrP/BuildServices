using System;
using System.Collections.Generic;
using System.Linq;
using Swashbuckle.Swagger;

namespace MyGetConnector.Swagger
{
    public class TriggerIdFilter : IOperationFilter
    {

        public void Apply(Operation operation, SchemaRegistry schemaRegistry, System.Web.Http.Description.ApiDescription apiDescription)
        {
            if (operation.operationId.IndexOf("Trigger", StringComparison.InvariantCultureIgnoreCase) < 0) return;

            var triggerStateParam = operation.parameters.FirstOrDefault(x => x.name.Equals("triggerId"));

            if (triggerStateParam == null) return;

            if (triggerStateParam.vendorExtensions == null)
            {
                triggerStateParam.vendorExtensions = new Dictionary<string, object>();
            }

            triggerStateParam.vendorExtensions.Add("x-ms-summary", "Trigger ID");
            triggerStateParam.vendorExtensions.Add("x-ms-visibility", "internal");
            triggerStateParam.vendorExtensions.Add("x-ms-scheduler-recommendation", "@workflow().name");
        }
    }
}