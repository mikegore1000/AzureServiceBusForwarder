using System;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.ServiceBus.Messaging;
using NUnit.Framework;
using static NServiceBus.AzureServiceBusForwarder.Tests.MessageFactory;

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

        [Test]
        public async Task when_forwarding_a_message_it_succeeds()
        {
            var forwarder = new MessageForwarder("DestinationQueue", endpointFake, message => typeof(TestMessage));
            var jsonMessage = await CreateMessageWithJsonBody();
            TestMessage forwardedMessage = null;
            A.CallTo(endpointFake).Invokes((object m, SendOptions o) => forwardedMessage = (TestMessage)m);

            await forwarder.FowardMessage(jsonMessage);

            Assert.That(forwardedMessage, Is.Not.Null);
        }
    }
}
