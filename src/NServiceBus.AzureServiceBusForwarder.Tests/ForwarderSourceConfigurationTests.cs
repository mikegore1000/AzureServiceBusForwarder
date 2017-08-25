using System;
using NUnit.Framework;

namespace NServiceBus.AzureServiceBusForwarder.Tests
{
    [TestFixture]
    public class ForwarderSourceConfigurationTests
    {
        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void when_creating_the_source_configuration_then_the_connection_string_is_required(string connectionString)
        {
            Assert.Throws<ArgumentException>(() => new ForwarderSourceConfiguration(connectionString, "TopicName"));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void when_creating_the_source_configuration_then_the_topic_name_is_required(string topicName)
        {
            Assert.Throws<ArgumentException>(() => new ForwarderSourceConfiguration("ConnectionString", topicName));
        }
    }
}
