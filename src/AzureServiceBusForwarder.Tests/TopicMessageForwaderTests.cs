using System;
using Microsoft.ServiceBus.Messaging;
using NUnit.Framework;

namespace AzureServiceBusForwarder.Tests
{
    [TestFixture]
    public class TopicMessageForwaderTests
    {
        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void when_creating_a_message_forwarder_the_topic_client_is_required(string destinationQueue)
        {
            Assert.Throws<ArgumentNullException>(() => new TopicMessageForwarder(null, message => { }));
        }

        [Test]
        public void when_creating_a_message_forwarder_a_null_message_mutator_does_not_cause_an_exception()
        {
            new TopicMessageForwarder(TopicClient.CreateFromConnectionString("Endpoint=sb://dummyns.servicebus.windows.net/", "TestTopic"), null);
        }
    }
}