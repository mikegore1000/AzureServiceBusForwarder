using System;

namespace AzureServiceBusForwarder
{
    public class ForwarderSourceConfiguration
    {
        public ForwarderSourceConfiguration(string connectionString, string topicName, int receiveBatchSize, Func<IBatchMessageReceiver> messageReceiverFactory)
        {
            Guard.IsNotNullOrEmpty(connectionString, nameof(connectionString));
            Guard.IsNotNullOrEmpty(topicName, nameof(topicName));
            Guard.IsGreaterThan(0, receiveBatchSize, nameof(receiveBatchSize));
            Guard.IsNotNull(messageReceiverFactory, nameof(messageReceiverFactory));

            ConnectionString = connectionString;
            TopicName = topicName;
            ReceiveBatchSize = receiveBatchSize;
            MessageReceiverFactory = messageReceiverFactory;
        }

        public string ConnectionString { get; }

        public string TopicName { get; }

        public int ReceiveBatchSize { get; }

        public Func<IBatchMessageReceiver> MessageReceiverFactory { get; }
    }
}