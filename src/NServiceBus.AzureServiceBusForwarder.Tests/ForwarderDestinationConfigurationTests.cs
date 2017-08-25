using System;
using FakeItEasy;
using NUnit.Framework;

namespace NServiceBus.AzureServiceBusForwarder.Tests
{
    [TestFixture]
    public class ForwarderDestinationConfigurationTests
    {
        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void when_creating_the_destination_configuration_then_the_destination_queue_is_required(string destinationQueue)
        {
            Assert.Throws<ArgumentException>(() => new ForwarderDestinationConfiguration(destinationQueue, A.Fake<IEndpointInstance>()));
        }

        [Test]
        public void when_creating_the_destination_configuration_then_the_endpoint_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new ForwarderDestinationConfiguration("DestinationQueue", null));
        }
    }
}
