using System;
using NUnit.Framework;

namespace AzureServiceBusForwarder.Tests
{
    [TestFixture]
    public class ForwarderConfigurationTests
    {
        [Test]
        public void when_creating_the_configuration_the_source_configuration_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new ForwarderConfiguration(null));
        }
    }
}
