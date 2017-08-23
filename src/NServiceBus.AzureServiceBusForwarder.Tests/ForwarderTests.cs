using System;
using FakeItEasy;
using NUnit.Framework;

namespace NServiceBus.AzureServiceBusForwarder.Tests
{
    [TestFixture]
    public class ForwarderTests
    {
        private IEndpointInstance endpointFake;

        [SetUp]
        public void Setup()
        {
            endpointFake = A.Fake<IEndpointInstance>();
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void when_creating_a_forwarder_the_connection_string_is_required(string connectionString)
        {
            Assert.Throws<ArgumentException>(() => new Forwarder(connectionString, "TestTopic", "TestSubscriber", endpointFake, message => typeof(TestMessage)));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void when_creating_a_forwarder_the_topic_name_is_required(string topicName)
        {
            Assert.Throws<ArgumentException>(() => new Forwarder("ConnectionString", topicName, "TestSubscriber", endpointFake, message => typeof(TestMessage)));
        }
    }

    public class TestMessage : IMessage
    {
    }
}
