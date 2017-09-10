using System;
using NUnit.Framework;

namespace AzureServiceBusForwarder.Tests
{
    [TestFixture]
    public class SubscriptionBatchMessageReceiverTests
    {
        [Test]
        public void when_creating_a_receiver_a_client_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new SubscriptionBatchMessageReceiver(null));
        }
    }
}