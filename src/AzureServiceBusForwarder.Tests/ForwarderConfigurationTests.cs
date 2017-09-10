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
                new ForwarderSourceConfiguration("ConnectionString", "TopicName", 1, A.Fake<IBatchMessageReceiver>),
                null));
        }

        [Test]
        public void when_creating_the_configuration_and_a_logger_is_specified_then_it_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new ForwarderConfiguration(
                new ForwarderSourceConfiguration("ConnectionString", "TopicName", 1, A.Fake<IBatchMessageReceiver>),
                new ForwarderDestinationConfiguration("DestinationQueue", A.Fake<IMessageForwarder>))
                .UsingLogger(null));
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        public void when_creating_the_configuration_and_the_concurrency_is_specified_then_it_must_be_a_positive_value(int concurrency)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ForwarderConfiguration(
                new ForwarderSourceConfiguration("ConnectionString", "TopicName", 1, A.Fake<IBatchMessageReceiver>),
                new ForwarderDestinationConfiguration("DestinationQueue", A.Fake<IMessageForwarder>))
                .WithConcurrencyOf(concurrency));
        }
    }
}
