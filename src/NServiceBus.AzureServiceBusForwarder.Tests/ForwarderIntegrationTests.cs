using System;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using NServiceBus.Logging;
using NUnit.Framework;
using static NServiceBus.AzureServiceBusForwarder.Tests.MessageFactory;

namespace NServiceBus.AzureServiceBusForwarder.Tests
{
    public class ForwarderIntegrationTests
    {
        private const string TopicName = "sourceTopic";
        private const string DestinationQueue = "destinationQueue";
        private IEndpointInstance endpointFake;
        private string namespaceConnectionString;
        private NamespaceManager namespaceManager;
        private Forwarder forwarder;


        [SetUp]
        public async Task Setup()
        {
            var loggerFake = A.Fake<ILog>();
            endpointFake = A.Fake<IEndpointInstance>();
            namespaceConnectionString = Environment.GetEnvironmentVariable("NServiceBus.AzureServiceBusForwarder.ConnectionString", EnvironmentVariableTarget.User);
            namespaceManager = NamespaceManager.CreateFromConnectionString(namespaceConnectionString);

            if (await namespaceManager.TopicExistsAsync(TopicName))
            {
                await namespaceManager.DeleteTopicAsync(TopicName);
            }

            await namespaceManager.CreateTopicAsync(TopicName);

            forwarder = new Forwarder(
                new ForwarderSourceConfiguration(namespaceConnectionString, TopicName, receiveBatchSize: 500, prefetchCount: 500),
                new ForwarderDestinationConfiguration(DestinationQueue, endpointFake),
                message => typeof(TestMessage),
                new AzureServiceBusForwarder.Serializers.JsonSerializer(),
                loggerFake);
        }

        [Test]
        public async Task when_a_forwarder_is_started_messages_are_forwarded_via_the_endpoint()
        {
            await forwarder.CreateSubscriptionEntitiesIfRequired();
            forwarder.Start();

            var topicClient = TopicClient.CreateFromConnectionString(namespaceConnectionString, TopicName);
            var eventMessage = await CreateMessageWithJsonBody();
            var tcs = new TaskCompletionSource<string>();
            await topicClient.SendAsync(eventMessage);

            A.CallTo(endpointFake).Invokes(() => tcs.SetResult("kfd"));

            if (!tcs.Task.Wait(TimeSpan.FromSeconds(10)))
            {
                Assert.Fail("Timed out waiting for message to be forwarded");
            }
        }

        [Test]
        public async Task when_a_forwarder_creates_subscription_entities_all_required_entities_are_created()
        {
            await forwarder.CreateSubscriptionEntitiesIfRequired();

            Assert.That(await namespaceManager.SubscriptionExistsAsync(TopicName, DestinationQueue), Is.True);
            Assert.That(await namespaceManager.QueueExistsAsync(DestinationQueue));
        }
    }
}