using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using NUnit.Framework;
using static NServiceBus.AzureServiceBusForwarder.Tests.QueueHelper;

namespace NServiceBus.AzureServiceBusForwarder.Tests
{
    [TestFixture]
    public class AzureServiceBusMessageForwarderTests
    {
        private QueueClient queueClient;

        [SetUp]
        public async Task Setup()
        {
            string destinationQueue = GetType().Name;
            await CreateQueue(destinationQueue);
            var queueConnectionString = Environment.GetEnvironmentVariable("NServiceBus.AzureServiceBusForwarder.ConnectionString", EnvironmentVariableTarget.User);
            queueClient = QueueClient.CreateFromConnectionString(queueConnectionString, destinationQueue);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void when_creating_a_message_forwarder_the_queue_client_is_required(string destinationQueue)
        {
            Assert.Throws<ArgumentNullException>(() => new AzureServiceBusMessageForwarder(null, message => { }));
        }

        [Test]
        public void when_creating_a_message_forwarder_a_null_message_forwarder_does_not_cause_an_exception()
        {
            new AzureServiceBusMessageForwarder(queueClient, null);
        }
    }
}