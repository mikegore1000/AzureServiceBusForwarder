using System;
using FakeItEasy;
using NUnit.Framework;

namespace NServiceBus.AzureServiceBusForwarder.Tests
{
    [TestFixture]
    public class MessageForwarderTests
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
        public void when_creating_a_message_forwarder_the_destination_queue_is_required(string destinationQueue)
        {
            Assert.Throws<ArgumentException>(() => new MessageForwarder(destinationQueue, endpointFake, message => typeof(TestMessage)));
        }

        [Test]
        public void when_creating_a_message_forwarder_an_endpoint_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new MessageForwarder("DestinationQueue", null, message => typeof(TestMessage)));
        }

        [Test]
        public void when_creating_a_message_forwarder_a_message_mapper_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new MessageForwarder("DestinationQueue", endpointFake, null));
        }
    }
}
