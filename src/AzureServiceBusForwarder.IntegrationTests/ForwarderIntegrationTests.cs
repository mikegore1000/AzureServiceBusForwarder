using System;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using NUnit.Framework;

namespace AzureServiceBusForwarder.IntegrationTests
{
    public class ForwarderIntegrationTests
    {
        private const string TopicName = "sourceTopic";
        private string destinationQueue;
        private IMessageForwarder messageForwarder;
        private string namespaceConnectionString;
        private NamespaceManager namespaceManager;
        private Forwarder forwarder;


        [SetUp]
        public async Task Setup()
        {
            destinationQueue = GetType().Name;
            var loggerFake = A.Fake<ILogger>();
            messageForwarder = A.Fake<IMessageForwarder>();

            await QueueHelper.CreateQueue(destinationQueue);

            namespaceConnectionString = Environment.GetEnvironmentVariable("NServiceBus.AzureServiceBusForwarder.ConnectionString", EnvironmentVariableTarget.User);
            namespaceManager = NamespaceManager.CreateFromConnectionString(namespaceConnectionString);

            if (await namespaceManager.TopicExistsAsync(TopicName))
            {
                await namespaceManager.DeleteTopicAsync(TopicName);
            }

            await namespaceManager.CreateTopicAsync(TopicName);

            forwarder = new Forwarder(
                new ForwarderSourceConfiguration(namespaceConnectionString, TopicName, receiveBatchSize: 500, prefetchCount: 500),
                new ForwarderDestinationConfiguration(destinationQueue, () => messageForwarder),
                loggerFake);
        }

        [Test]
        public async Task when_a_forwarder_is_started_messages_are_forwarded_via_the_endpoint()
        {
            await forwarder.CreateSubscriptionEntitiesIfRequired();
            forwarder.Start();

            var topicClient = TopicClient.CreateFromConnectionString(namespaceConnectionString, TopicName);
            var eventMessage = await MessageFactory.CreateMessageWithJsonBody();
            var tcs = new TaskCompletionSource<string>();
            await topicClient.SendAsync(eventMessage);

            A.CallTo(messageForwarder).Invokes(() => tcs.SetResult("kfd"));

            if (!tcs.Task.Wait(TimeSpan.FromSeconds(10)))
            {
                Assert.Fail("Timed out waiting for message to be forwarded");
            }
        }

        [Test]
        public async Task when_a_forwarder_creates_subscription_entities_all_required_entities_are_created()
        {
            await forwarder.CreateSubscriptionEntitiesIfRequired();

            Assert.That(await namespaceManager.SubscriptionExistsAsync(TopicName, destinationQueue), Is.True);
            Assert.That(await namespaceManager.QueueExistsAsync(destinationQueue));
        }
    }
}