using System;
using FakeItEasy;
using NUnit.Framework;

namespace AzureServiceBusForwarder.Tests
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
            Assert.Throws<ArgumentException>(() => new ForwarderSourceConfiguration(connectionString, "TopicName", ReceiveBatchSize, A.Fake<IBatchMessageReceiver>));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void when_creating_the_source_configuration_then_the_topic_name_is_required(string topicName)
        {
            Assert.Throws<ArgumentException>(() => new ForwarderSourceConfiguration("ConnectionString", topicName, ReceiveBatchSize, A.Fake<IBatchMessageReceiver>));
        }

        [Test]
        [TestCase(-ReceiveBatchSize)]
        [TestCase(0)]
        public void when_creating_the_source_configuration_then_the_receive_batch_size_must_be_greater_than_zero(int batchSize)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ForwarderSourceConfiguration("ConnectionString", "TopicName", batchSize, A.Fake<IBatchMessageReceiver>));
        }

        [Test]
        public void when_creating_the_source_configuration_then_the_message_receiver_factory_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new ForwarderSourceConfiguration("ConnectionString", "TopicName", ReceiveBatchSize, null));
        }        
    }
}
