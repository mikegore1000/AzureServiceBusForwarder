namespace AzureServiceBusForwarder
{
    public class ForwarderSourceConfiguration
    {
        public ForwarderSourceConfiguration(string connectionString, string topicName, int receiveBatchSize, int prefetchCount)
        {
            Guard.IsNotNullOrEmpty(connectionString, nameof(connectionString));
            Guard.IsNotNullOrEmpty(topicName, nameof(topicName));
            Guard.IsGreaterThan(0, receiveBatchSize, nameof(receiveBatchSize));
            Guard.IsGreaterThan(0, prefetchCount, nameof(prefetchCount));

            ConnectionString = connectionString;
            TopicName = topicName;
            ReceiveBatchSize = receiveBatchSize;
            PrefetchCount = prefetchCount;
        }

        public string ConnectionString { get; }

        public string TopicName { get; }

        public int ReceiveBatchSize { get; }

        public int PrefetchCount { get; }
    }
}