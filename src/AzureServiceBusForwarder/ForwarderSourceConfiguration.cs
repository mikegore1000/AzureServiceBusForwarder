using System;

namespace AzureServiceBusForwarder
{
    public class ForwarderSourceConfiguration
    {
        public ForwarderSourceConfiguration(int receiveBatchSize, Func<IBatchMessageReceiver> messageReceiverFactory)
        {
            Guard.IsGreaterThan(0, receiveBatchSize, nameof(receiveBatchSize));
            Guard.IsNotNull(messageReceiverFactory, nameof(messageReceiverFactory));

            ReceiveBatchSize = receiveBatchSize;
            MessageReceiverFactory = messageReceiverFactory;
        }

        public string ConnectionString { get; }

        public string TopicName { get; }

        public int ReceiveBatchSize { get; }

        public Func<IBatchMessageReceiver> MessageReceiverFactory { get; }
    }
}