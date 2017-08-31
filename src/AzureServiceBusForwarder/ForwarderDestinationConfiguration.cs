using System;

namespace NServiceBus.AzureServiceBusForwarder
{
    public class ForwarderDestinationConfiguration
    {
        public ForwarderDestinationConfiguration(string destinationQueue, Func<IMessageForwarder> messageForwarderFactory)
        {
            Guard.IsNotNullOrEmpty(destinationQueue, nameof(destinationQueue));
            Guard.IsNotNull(messageForwarderFactory, nameof(messageForwarderFactory));

            DestinationQueue = destinationQueue;
            MessageForwarderFactory = messageForwarderFactory;
        }

        public string DestinationQueue { get; }

        public Func<IMessageForwarder> MessageForwarderFactory { get; }
    }
}
