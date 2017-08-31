using System;
using NUnit.Framework;

namespace AzureServiceBusForwarder.Tests
{
    [TestFixture]
    public class ForwarderTests
    {
        [Test]
        public void when_creating_a_forwarder_the_configuration_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new Forwarder(null));
        }
    }
}
