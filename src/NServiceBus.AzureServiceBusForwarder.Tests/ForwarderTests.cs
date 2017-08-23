using System;
using FakeItEasy;
using Microsoft.ServiceBus;
using NUnit.Framework;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace NServiceBus.AzureServiceBusForwarder.Tests
{
    [TestFixture]
    public class ForwarderTests
    {
        private IEndpointInstance endpointFake;

        [SetUp]
        public void Setup()
        {
            endpointFake = A.Fake<IEndpointInstance>();
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void when_creating_a_forwarder_the_connection_string_is_required(string connectionString)
        {
            Assert.Throws<ArgumentException>(() => new Forwarder(connectionString, "TestTopic", "DestinationQueue", endpointFake, message => typeof(TestMessage)));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void when_creating_a_forwarder_the_topic_name_is_required(string topicName)
        {
            Assert.Throws<ArgumentException>(() => new Forwarder("ConnectionString", topicName, "DestinationQueue", endpointFake, message => typeof(TestMessage)));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void when_creating_a_forwarder_the_destination_queue_is_required(string destinationQueue)
        {
            Assert.Throws<ArgumentException>(() => new Forwarder("ConnectionString", "TestTopic", destinationQueue, endpointFake, message => typeof(TestMessage)));
        }

        [Test]
        public void when_creating_a_forwarder_an_endpoint_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new Forwarder("ConnectionString", "TestTopic", "DestinationQueue", null, message => typeof(TestMessage)));
        }

        [Test]
        public void when_creating_a_forwarder_a_message_mapper_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new Forwarder("ConnectionString", "TestTopic", "DestinationQueue", endpointFake, null));
        }

        [Test]
        public async Task when_a_forwarder_is_started_messages_are_forwarded_via_the_endpoint()
        {
            const string topicName = "sourceTopic";
            var namespaceConnectionString = Environment.GetEnvironmentVariable("NServiceBus.AzureServiceBusForwarder.ConnectionString", EnvironmentVariableTarget.User);
            var namespaceManager = NamespaceManager.CreateFromConnectionString(namespaceConnectionString);
            
            if (!await namespaceManager.TopicExistsAsync(topicName))
            {
                await namespaceManager.CreateTopicAsync(topicName);
            }

            var forwarder = new Forwarder(namespaceConnectionString, topicName, "destinationQueue", endpointFake, message => typeof(TestMessage));
            await forwarder.Start();

            var topicClient = TopicClient.CreateFromConnectionString(namespaceConnectionString, topicName);
            var eventMessage = new BrokeredMessage(new TestMessage());
            var tcs = new TaskCompletionSource<string>();
            await topicClient.SendAsync(eventMessage);

            A.CallTo(endpointFake).Invokes(() => tcs.SetResult("kfd"));

            if (!tcs.Task.Wait(TimeSpan.FromSeconds(10)))
            {
                Assert.Fail("Timed out waiting for message to be forwarded");
            }
        }
    }
}
