using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using NUnit.Framework;

namespace AzureServiceBusForwarder.IntegrationTests
{
    [TestFixture]
    public class BatchMessageReceiverIntegrationTests
    {
        private QueueClient queueClient;

        [SetUp]
        public async Task Setup()
        {
            string destinationQueue = GetType().Name;
            await MessageEntityHelper.CreateQueue(destinationQueue);
            var queueConnectionString = Environment.GetEnvironmentVariable("NServiceBus.AzureServiceBusForwarder.ConnectionString", EnvironmentVariableTarget.User);
            queueClient = QueueClient.CreateFromConnectionString(queueConnectionString, destinationQueue);
        }

        [Test]
        public async Task when_receiving_a_batch_messages_are_returned_and_can_be_completed()
        {   
            var message = await MessageFactory.CreateMessageWithJsonBody();
            
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
            var receiver = new BatchMessageReceiver(queueClient);

            await receiver.CompleteMessages(new Guid[] {});
        }
    }
}