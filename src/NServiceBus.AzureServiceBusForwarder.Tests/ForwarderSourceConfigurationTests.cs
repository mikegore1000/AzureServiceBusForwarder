using System;
using NUnit.Framework;

namespace NServiceBus.AzureServiceBusForwarder.Tests
{
    [TestFixture]
    public class ForwarderSourceConfigurationTests
    {
        private const int ReceiveBatchSize = 1;

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void when_creating_the_source_configuration_then_the_connection_string_is_required(string connectionString)
        {
            Assert.Throws<ArgumentException>(() => new ForwarderSourceConfiguration(connectionString, "TopicName", ReceiveBatchSize));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void when_creating_the_source_configuration_then_the_topic_name_is_required(string topicName)
        {
            Assert.Throws<ArgumentException>(() => new ForwarderSourceConfiguration("ConnectionString", topicName, ReceiveBatchSize));
        }

        [Test]
        [TestCase(-ReceiveBatchSize)]
        [TestCase(0)]
        public void when_creating_the_source_configuration_then_the_receive_batch_size_must_be_greater_than_zero(int batchSize)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ForwarderSourceConfiguration("ConnectionString", "TopicName", batchSize));
        }
    }
}
