namespace Microsoft.Its.Recipes
{
    internal static partial class Any
    {
        internal static partial class Azure
        {
            internal static partial class AppServiceTrigger
            {
                internal static object MyGetConnectorTriggerBody(string callbackUrl = null)
                {
                    return new
                    {
                        callbackUrl = callbackUrl ?? Any.Uri().ToString(),
                        inputs = Any.String()
                    };
                }
            }
        }
    }
}
