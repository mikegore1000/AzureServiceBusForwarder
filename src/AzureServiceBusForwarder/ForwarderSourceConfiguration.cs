namespace AzureServiceBusForwarder
{
    public class ForwarderSourceConfiguration
    {
        public ForwarderSourceConfiguration(string connectionString, string topicName, int receiveBatchSize)
        {
            Guard.IsNotNullOrEmpty(connectionString, nameof(connectionString));
            Guard.IsNotNullOrEmpty(topicName, nameof(topicName));
            Guard.IsGreaterThan(0, receiveBatchSize, nameof(receiveBatchSize));

            ConnectionString = connectionString;
            TopicName = topicName;
            ReceiveBatchSize = receiveBatchSize;
        }

        public string ConnectionString { get; }

        public string TopicName { get; }

        public int ReceiveBatchSize { get; }
    }
}