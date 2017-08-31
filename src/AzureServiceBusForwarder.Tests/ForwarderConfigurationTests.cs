using System;
using FakeItEasy;
using NUnit.Framework;

namespace AzureServiceBusForwarder.Tests
{
    [TestFixture]
    public class ForwarderConfigurationTests
    {
        [Test]
        public void when_creating_the_configuration_the_source_configuration_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new ForwarderConfiguration(
                null,
                new ForwarderDestinationConfiguration("DestinationQueue", A.Fake<IMessageForwarder>)));
        }

        [Test]
        public void when_creating_the_configuration_the_destination_configuration_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new ForwarderConfiguration(
                new ForwarderSourceConfiguration("ConnectionString", "TopicName", 1),
                null));
        }
    }
}
