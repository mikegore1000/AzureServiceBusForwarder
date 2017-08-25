using System;
using FakeItEasy;
using NUnit.Framework;
using Serializer = NServiceBus.AzureServiceBusForwarder.Serializers;

namespace NServiceBus.AzureServiceBusForwarder.Tests
{
    [TestFixture]
    public class ForwarderTests
    {
        private const int ReceiveBatchSize = 1;
        private IEndpointInstance endpointFake;

        [SetUp]
        public void Setup()
        {
            endpointFake = A.Fake<IEndpointInstance>();
        }

        [Test]
        public void when_creating_a_forwarder_the_source_configuration_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new Forwarder(
                null,
                new ForwarderDestinationConfiguration("DestinationQueue", endpointFake),
                message => typeof(TestMessage),
                new Serializer.JsonSerializer()));
        }

        [Test]
        public void when_creating_a_forwarder_the_destination_configuration_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new Forwarder(
                new ForwarderSourceConfiguration("ConnectionString", "TestTopic", ReceiveBatchSize),
                null,
                message => typeof(TestMessage),
                new Serializer.JsonSerializer()));
        }

        [Test]
        public void when_creating_a_forwarder_a_message_mapper_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new Forwarder(
                new ForwarderSourceConfiguration("ConnectionString", "TestTopic", ReceiveBatchSize),
                new ForwarderDestinationConfiguration("DestinationQueue", endpointFake),
                null,
                new Serializer.JsonSerializer()));
        }

        [Test]
        public void when_creating_a_forwarder_a_serializer_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new Forwarder(
                new ForwarderSourceConfiguration("ConnectionString", "TestTopic", ReceiveBatchSize),
                new ForwarderDestinationConfiguration("DestinationQueue", endpointFake), 
                message => typeof(TestMessage),
                null));
        }
    }
}
