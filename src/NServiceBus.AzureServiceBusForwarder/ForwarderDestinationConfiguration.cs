namespace NServiceBus.AzureServiceBusForwarder
{
    public class ForwarderDestinationConfiguration
    {
        public ForwarderDestinationConfiguration(string destinationQueue, IEndpointInstance endpoint)
        {
            Guard.IsNotNullOrEmpty(destinationQueue, nameof(destinationQueue));
            Guard.IsNotNull(endpoint, nameof(endpoint));

            DestinationQueue = destinationQueue;
            Endpoint = endpoint;
        }

        public string DestinationQueue { get; }

        public IEndpointInstance Endpoint { get; }
    }
}
