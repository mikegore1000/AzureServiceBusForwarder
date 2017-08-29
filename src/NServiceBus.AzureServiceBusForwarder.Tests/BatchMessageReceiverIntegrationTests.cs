using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using NUnit.Framework;
using static NServiceBus.AzureServiceBusForwarder.Tests.MessageFactory;

namespace NServiceBus.AzureServiceBusForwarder.Tests
{
    [TestFixture]
    public class BatchMessageReceiverIntegrationTests
    {
        [Test]
        public async Task when_receiving_a_batch_messages_are_returned_and_can_be_completed()
        {
            const string destinationQueue = "destinationQueue";
            var queueConnectionString = Environment.GetEnvironmentVariable("NServiceBus.AzureServiceBusForwarder.ConnectionString", EnvironmentVariableTarget.User);
            var message = await CreateMessageWithJsonBody();
            var queueClient = QueueClient.CreateFromConnectionString(queueConnectionString, destinationQueue);
            var receiver = new BatchMessageReceiver(queueClient);
            await queueClient.SendAsync(message);

            var messages = await receiver.ReceieveMessages(1);

            Assert.That(messages, Is.Not.Null);
            Assert.That(messages.Count, Is.EqualTo(1));
            await receiver.CompleteMessages(new [] { messages.First().LockToken });
        }

        [Test]
        public async Task when_completing_with_empty_message_tokens_then_no_exceptions_are_thrown()
        {
            const string destinationQueue = "destinationQueue";
            var queueConnectionString = Environment.GetEnvironmentVariable("NServiceBus.AzureServiceBusForwarder.ConnectionString", EnvironmentVariableTarget.User);
            var queueClient = QueueClient.CreateFromConnectionString(queueConnectionString, destinationQueue);
            var receiver = new BatchMessageReceiver(queueClient);

            await receiver.CompleteMessages(new Guid[] {});
        }
    }
}