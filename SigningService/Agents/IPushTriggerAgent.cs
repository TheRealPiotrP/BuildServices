namespace SigningService.Agents
{
    internal interface IPushTriggerAgent
    {
        void FirePackageSignedTrigger(object signedPackageUri);
    }
}