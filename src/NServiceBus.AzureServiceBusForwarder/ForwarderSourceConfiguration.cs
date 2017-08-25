namespace NServiceBus.AzureServiceBusForwarder
{
    public class ForwarderSourceConfiguration
    {
        public ForwarderSourceConfiguration(string connectionString, string topicName)
        {
            Guard.IsNotNullOrEmpty(connectionString, nameof(connectionString));
            Guard.IsNotNullOrEmpty(topicName, nameof(topicName));

            ConnectionString = connectionString;
            TopicName = topicName;
        }

        public string ConnectionString { get; }

        public string TopicName { get; }
    }
}