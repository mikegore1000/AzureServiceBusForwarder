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
            Assert.Throws<ArgumentException>(() => new Forwarder(connectionString, "TestTopic", "DestinationQueue", endpointFake, message => typeof(TestMessage)));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void when_creating_a_forwarder_the_topic_name_is_required(string topicName)
        {
            Assert.Throws<ArgumentException>(() => new Forwarder("ConnectionString", topicName, "DestinationQueue", endpointFake, message => typeof(TestMessage)));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void when_creating_a_forwarder_the_destination_queue_is_required(string destinationQueue)
        {
            Assert.Throws<ArgumentException>(() => new Forwarder("ConnectionString", "TestTopic", destinationQueue, endpointFake, message => typeof(TestMessage)));
        }

        [Test]
        public void when_creating_a_forwarder_an_endpoint_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new Forwarder("ConnectionString", "TestTopic", "DestinationQueue", null, message => typeof(TestMessage)));
        }

        [Test]
        public void when_creating_a_forwarder_a_message_mapper_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new Forwarder("ConnectionString", "TestTopic", "DestinationQueue", endpointFake, null));
        }

        [Test]
        public void when_providing_a_serializer_it_is_required()
        {
            var forwarder = new Forwarder("ConnectionString", "TestTopic", "DestinationQueue", endpointFake, message => typeof(TestMessage));
            Assert.Throws<ArgumentNullException>(() => forwarder.SetSerializer(null));
        }
    }
}
