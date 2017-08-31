namespace AzureServiceBusForwarder
{
    public class ForwarderConfiguration
    {
        private readonly ForwarderSourceConfiguration source;

        public ForwarderConfiguration(ForwarderSourceConfiguration source)
        {
            Guard.IsNotNull(source, nameof(source));

            this.source = source;
        }
    }
}
