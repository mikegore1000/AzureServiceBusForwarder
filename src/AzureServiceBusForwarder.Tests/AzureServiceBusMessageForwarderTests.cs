using System;
using Microsoft.ServiceBus.Messaging;
using NUnit.Framework;

namespace NServiceBus.AzureServiceBusForwarder.Tests
{
    [TestFixture]
    public class AzureServiceBusMessageForwarderTests
    {
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
            new AzureServiceBusMessageForwarder(QueueClient.CreateFromConnectionString("Endpoint=sb://dummyns.servicebus.windows.net/", "TestQueue"), null);
        }
    }
}