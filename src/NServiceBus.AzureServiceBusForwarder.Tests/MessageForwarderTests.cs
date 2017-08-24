using System;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.ServiceBus.Messaging;
using NUnit.Framework;
using static NServiceBus.AzureServiceBusForwarder.Tests.MessageFactory;
using Serializer = NServiceBus.AzureServiceBusForwarder.Serializers;

namespace NServiceBus.AzureServiceBusForwarder.Tests
{
    [TestFixture]
    public class MessageForwarderTests
    {
        private IEndpointInstance endpointFake;
        private MessageForwarder forwarder;
        private BrokeredMessage jsonMessage;

        [SetUp]
        public async Task Setup()
        {
            endpointFake = A.Fake<IEndpointInstance>();
            forwarder = new MessageForwarder("DestinationQueue", endpointFake, message => typeof(TestMessage), new Serializer.JsonSerializer());
            jsonMessage = await CreateMessageWithJsonBody();
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void when_creating_a_message_forwarder_the_destination_queue_is_required(string destinationQueue)
        {
            Assert.Throws<ArgumentException>(() => new MessageForwarder(destinationQueue, endpointFake, message => typeof(TestMessage), new Serializer.JsonSerializer()));
        }

        [Test]
        public void when_creating_a_message_forwarder_an_endpoint_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new MessageForwarder("DestinationQueue", null, message => typeof(TestMessage), new Serializer.JsonSerializer()));
        }

        [Test]
        public void when_creating_a_message_forwarder_a_message_mapper_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new MessageForwarder("DestinationQueue", endpointFake, null, new Serializer.JsonSerializer()));
        }

        [Test]
        public void when_creating_a_message_forwarder_a_serializer_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new MessageForwarder("DestinationQueue", endpointFake, message => typeof(TestMessage), null));
        }

        [Test]
        public async Task when_forwarding_a_message_it_succeeds()
        {
            TestMessage forwardedMessage = null;
            A.CallTo(endpointFake).Invokes((object m, SendOptions o) => forwardedMessage = (TestMessage)m);

            await forwarder.FowardMessage(jsonMessage);

            Assert.That(forwardedMessage, Is.Not.Null);
        }

        [Test]
        public async Task when_forwarding_a_message_with_an_ignored_header_it_is_not_copied_to_the_message()
        {
            jsonMessage.Properties.Add("NServiceBus.Transport.Encoding", "Test");
            SendOptions sendOptions = null;
            A.CallTo(endpointFake).Invokes((object m, SendOptions o) => sendOptions = o);

            await forwarder.FowardMessage(jsonMessage);

            Assert.That(sendOptions.GetHeaders().ContainsKey("NServiceBus.Transport.Encoding"), Is.False);
        }

        [Test]
        public async Task when_forwarding_a_message_with_a_custom_header_it_is_copied_to_the_message()
        {
            jsonMessage.Properties.Add("TestHeader", "Test");
            SendOptions sendOptions = null;
            A.CallTo(endpointFake).Invokes((object m, SendOptions o) => sendOptions = o);

            await forwarder.FowardMessage(jsonMessage);

            Assert.That(sendOptions.GetHeaders().ContainsKey("TestHeader"), Is.True);
        }

        [Test]
        public async Task when_forwarding_a_message_the_serializer_is_used()
        {
            bool serializerCalled = false;
            var customSerializer = new TestJsonSerializer(() => serializerCalled = true);
            forwarder = new MessageForwarder("DestinationQueue", endpointFake, message => typeof(TestMessage), customSerializer);

            await forwarder.FowardMessage(jsonMessage);

            Assert.That(serializerCalled, Is.True);
        }

        private class TestJsonSerializer : Serializer.ISerializer
        {
            private readonly Serializer.JsonSerializer serializer = new Serializer.JsonSerializer();
            private readonly Action onDeserialize;

            public TestJsonSerializer(Action onDeserialize)
            {
                this.onDeserialize = onDeserialize;
            }

            public bool CanDeserialize(BrokeredMessage message)
            {
                return serializer.CanDeserialize(message);
            }

            public object Deserialize(BrokeredMessage message, Type type)
            {
                onDeserialize();
                return serializer.Deserialize(message, type);
            }
        }
    }
}
