using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using NUnit.Framework;

namespace AzureServiceBusForwarder.IntegrationTests
{    
    [TestFixture]
    public class QueueForwarderIntegrationTests
    {
        private QueueClient queueClient;
        private IEnumerable<Guid> returnedLockTokens;
        private IEnumerable<BrokeredMessage> forwardedMessages;

        [SetUp]
        public async Task Setup()
        {
            string destinationQueue = GetType().Name;
            await QueueHelper.CreateQueue(destinationQueue);
            var connectionString = Environment.GetEnvironmentVariable("NServiceBus.AzureServiceBusForwarder.ConnectionString", EnvironmentVariableTarget.User);
            queueClient = QueueClient.CreateFromConnectionString(connectionString, destinationQueue);
        }

        [TearDown]
        public async Task TearDown()
        {
            if (returnedLockTokens != null && returnedLockTokens.Any())
            {
                await queueClient.CompleteBatchAsync(returnedLockTokens);
            }

            if (forwardedMessages != null && forwardedMessages.Any())
            {
                await queueClient.CompleteBatchAsync(forwardedMessages.Select(x => x.LockToken));
            }
        }

        [Test]
        public async Task when_forwarding_multiple_messages_all_lock_tokens_are_returned_and_the_messages_are_forwarded()
        {
            var forwarder = new AzureServiceBusMessageForwarder(queueClient, null);
            var messagesToForward = new List<BrokeredMessage>();
            messagesToForward.Add(await MessageFactory.CreateMessageWithJsonBody());
            messagesToForward.Add(await MessageFactory.CreateMessageWithJsonBody());
            await queueClient.SendBatchAsync(messagesToForward);

            var receivedMessages = await queueClient.ReceiveBatchAsync(100);
            returnedLockTokens = await forwarder.ForwardMessages(receivedMessages);
            forwardedMessages = await queueClient.ReceiveBatchAsync(100);

            Assert.That(returnedLockTokens.Count(), Is.EqualTo(2));
            CollectionAssert.AllItemsAreUnique(returnedLockTokens);
            Assert.That(forwardedMessages.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task when_forwarding_a_message_with_a_custom_header_it_is_copied_to_the_message()
        {
            var forwarder = new AzureServiceBusMessageForwarder(queueClient, m => m.Properties["Test"] = "Value");
            var messageToForward = await MessageFactory.CreateMessageWithJsonBody();
            queueClient.Send(messageToForward);

            var receivedMessages = await queueClient.ReceiveBatchAsync(100, TimeSpan.FromSeconds(3));
            returnedLockTokens = await forwarder.ForwardMessages(receivedMessages);
            forwardedMessages = await queueClient.ReceiveBatchAsync(100, TimeSpan.FromSeconds(3));

            Assert.That(forwardedMessages.First().Properties.ContainsKey("Test"), Is.True);
            Assert.That(forwardedMessages.First().Properties["Test"], Is.EqualTo("Value"));
        }

        [Test]
        public async Task when_forwarding_a_message_standard_headers_are_copied()
        {
            var messageId = Guid.NewGuid().ToString();
            var contentType = "application/json";

            var forwarder = new AzureServiceBusMessageForwarder(queueClient, m => m.Properties["Test"] = "Value");
            var messageToForward = await MessageFactory.CreateMessageWithJsonBody();
            messageToForward.MessageId = messageId;
            messageToForward.ContentType = contentType;

            queueClient.Send(messageToForward);

            var receivedMessages = await queueClient.ReceiveBatchAsync(100, TimeSpan.FromSeconds(3));
            returnedLockTokens = await forwarder.ForwardMessages(receivedMessages);
            forwardedMessages = await queueClient.ReceiveBatchAsync(100, TimeSpan.FromSeconds(3));

            Assert.That(forwardedMessages.First().MessageId, Is.EqualTo(messageId));
            Assert.That(forwardedMessages.First().ContentType, Is.EqualTo(contentType));
        }
    }
}