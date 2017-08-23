using System;
using FakeItEasy;
using NUnit.Framework;

namespace NServiceBus.AzureServiceBusForwarder.Tests
{
    [TestFixture]
    public class ForwarderTests
    {
        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void when_creating_a_forwarder_the_connection_string_is_required(string connectionString)
        {
            var endpoint = A.Fake<IEndpointInstance>();
            Assert.Throws<ArgumentException>(() => new Forwarder(connectionString, "TestTopic", "TestSubscriber", endpoint, message => typeof(TestMessage)));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void when_creating_a_forwarder_the_topic_name_is_required(string topicName)
        {
            var endpoint = A.Fake<IEndpointInstance>();
            Assert.Throws<ArgumentException>(() => new Forwarder("ConnectionString", topicName, "TestSubscriber", endpoint, message => typeof(TestMessage)));
        }
    }

    public class TestMessage : IMessage
    {
    }
}
