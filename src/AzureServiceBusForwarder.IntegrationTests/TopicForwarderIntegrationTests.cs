using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using NUnit.Framework;

namespace AzureServiceBusForwarder.IntegrationTests
{
    [TestFixture]
    public class TopicForwarderIntegrationTests
    {
        private TopicClient topicClient;
        private SubscriptionClient subscriptionClient;
        private IEnumerable<Guid> returnedLockTokens;
        private IEnumerable<BrokeredMessage> forwardedMessages;

        [SetUp]
        public async Task Setup()
        {
            string destinationTopic = GetType().Name;
            await MessageEntityHelper.CreateTopicWithSubscription(destinationTopic);
            var connectionString = Environment.GetEnvironmentVariable("NServiceBus.AzureServiceBusForwarder.ConnectionString", EnvironmentVariableTarget.User);
            topicClient = TopicClient.CreateFromConnectionString(connectionString, destinationTopic);
            subscriptionClient = SubscriptionClient.CreateFromConnectionString(connectionString, destinationTopic, destinationTopic);
        }

        [TearDown]
        public async Task TearDown()
        {
            if (returnedLockTokens != null && returnedLockTokens.Any())
            {
                await subscriptionClient.CompleteBatchAsync(returnedLockTokens);
            }

            if (forwardedMessages != null && forwardedMessages.Any())
            {
                await subscriptionClient.CompleteBatchAsync(forwardedMessages.Select(x => x.LockToken));
            }
        }

        [Test]
        public async Task when_forwarding_multiple_messages_all_lock_tokens_are_returned_and_the_messages_are_forwarded()
        {
            var forwarder = new TopicMessageForwarder(topicClient, null);
            var messagesToForward = new List<BrokeredMessage>();
            messagesToForward.Add(await MessageFactory.CreateMessageWithJsonBody());
            messagesToForward.Add(await MessageFactory.CreateMessageWithJsonBody());
            await topicClient.SendBatchAsync(messagesToForward);

            var receivedMessages = await subscriptionClient.ReceiveBatchAsync(100);
            returnedLockTokens = await forwarder.ForwardMessages(receivedMessages);
            forwardedMessages = await subscriptionClient.ReceiveBatchAsync(100);

            Assert.That(returnedLockTokens.Count(), Is.EqualTo(2));
            CollectionAssert.AllItemsAreUnique(returnedLockTokens);
            Assert.That(forwardedMessages.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task when_forwarding_a_message_with_a_custom_header_it_is_copied_to_the_message()
        {
            var forwarder = new TopicMessageForwarder(topicClient, m => m.Properties["Test"] = "Value");
            var messageToForward = await MessageFactory.CreateMessageWithJsonBody();
            topicClient.Send(messageToForward);

            var receivedMessages = await subscriptionClient.ReceiveBatchAsync(100, TimeSpan.FromSeconds(3));
            returnedLockTokens = await forwarder.ForwardMessages(receivedMessages);
            forwardedMessages = await subscriptionClient.ReceiveBatchAsync(100, TimeSpan.FromSeconds(3));

            Assert.That(forwardedMessages.First().Properties.ContainsKey("Test"), Is.True);
            Assert.That(forwardedMessages.First().Properties["Test"], Is.EqualTo("Value"));
        }

        [Test]
        public async Task when_forwarding_a_message_standard_headers_are_copied()
        {
            var messageId = Guid.NewGuid().ToString();
            var contentType = "application/json";

            var forwarder = new TopicMessageForwarder(topicClient, m => m.Properties["Test"] = "Value");
            var messageToForward = await MessageFactory.CreateMessageWithJsonBody();
            messageToForward.MessageId = messageId;
            messageToForward.ContentType = contentType;

            topicClient.Send(messageToForward);

            var receivedMessages = await subscriptionClient.ReceiveBatchAsync(100, TimeSpan.FromSeconds(3));
            returnedLockTokens = await forwarder.ForwardMessages(receivedMessages);
            forwardedMessages = await subscriptionClient.ReceiveBatchAsync(100, TimeSpan.FromSeconds(3));

            Assert.That(forwardedMessages.First().MessageId, Is.EqualTo(messageId));
            Assert.That(forwardedMessages.First().ContentType, Is.EqualTo(contentType));
        }
    }
}