using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.ServiceBus.Messaging;
using NUnit.Framework;

namespace NServiceBus.AzureServiceBusForwarder.Tests
{
    [TestFixture]
    public class MessageForwarderIntegrationTests
    {
        private IEndpointInstance endpointFake;
        private MessageForwarder forwarder;
        private QueueClient queueClient;

        [SetUp]
        public void Setup()
        {
            const string destinationQueue = "destinationQueue";
            var queueConnectionString = Environment.GetEnvironmentVariable("NServiceBus.AzureServiceBusForwarder.ConnectionString", EnvironmentVariableTarget.User);
            queueClient = QueueClient.CreateFromConnectionString(queueConnectionString, destinationQueue);

            endpointFake = A.Fake<IEndpointInstance>();
            forwarder = new MessageForwarder("DestinationQueue", endpointFake, message => typeof(TestMessage), new AzureServiceBusForwarder.Serializers.JsonSerializer());
        }

        [Test]
        public async Task when_forwarding_multiple_messages_all_lock_tokens_are_returned()
        {
            var messagesToForward = new List<BrokeredMessage>();
            messagesToForward.Add(await MessageFactory.CreateMessageWithJsonBody());
            messagesToForward.Add(await MessageFactory.CreateMessageWithJsonBody());            
            await queueClient.SendBatchAsync(messagesToForward);
            var receivedMessages = await queueClient.ReceiveBatchAsync(100);
            int messagesForwarded = 0;
            A.CallTo(endpointFake).Invokes(() => Interlocked.Increment(ref messagesForwarded));

            var returnedLockTokens = await forwarder.ForwardMessages(receivedMessages);

            Assert.That(messagesForwarded, Is.EqualTo(2));
            Assert.That(returnedLockTokens.Count(), Is.EqualTo(2));
            CollectionAssert.AllItemsAreUnique(returnedLockTokens);
        }
    }
}