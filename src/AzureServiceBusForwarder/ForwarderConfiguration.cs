namespace AzureServiceBusForwarder
{
    public class ForwarderConfiguration
    {
        private readonly ForwarderSourceConfiguration source;
        private readonly ForwarderDestinationConfiguration destination;

        public ForwarderConfiguration(ForwarderSourceConfiguration source, ForwarderDestinationConfiguration destination)
        {
            Guard.IsNotNull(source, nameof(source));
            Guard.IsNotNull(destination, nameof(destination));

            this.source = source;
            this.destination = destination;
        }
    }
}
