using System;
using NUnit.Framework;

namespace NServiceBus.AzureServiceBusForwarder.Tests
{
    [TestFixture]
    public class BatchMessageReceiverTests
    {
        [Test]
        public void when_creating_a_receiver_a_client_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new BatchMessageReceiver(null));
        }
    }
}
