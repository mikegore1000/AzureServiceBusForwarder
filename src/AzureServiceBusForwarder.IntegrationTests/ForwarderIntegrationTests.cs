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
        private IBatchMessageReceiver messageReceiver;


        [SetUp]
        public async Task Setup()
        {
            namespaceConnectionString = Environment.GetEnvironmentVariable("NServiceBus.AzureServiceBusForwarder.ConnectionString", EnvironmentVariableTarget.User);
            destinationQueue = GetType().Name;
            messageForwarder = A.Fake<IMessageForwarder>();
            messageReceiver = new QueueBatchMessageReceiver(QueueClient.CreateFromConnectionString(namespaceConnectionString, destinationQueue));

            await MessageEntityHelper.CreateQueue(destinationQueue);
            
            namespaceManager = NamespaceManager.CreateFromConnectionString(namespaceConnectionString);

            if (await namespaceManager.TopicExistsAsync(TopicName))
            {
                await namespaceManager.DeleteTopicAsync(TopicName);
            }

            await namespaceManager.CreateTopicAsync(TopicName);

            forwarder = new Forwarder(
                new ForwarderConfiguration(
                    new ForwarderSourceConfiguration(namespaceConnectionString, TopicName, 500, () => messageReceiver),
                    new ForwarderDestinationConfiguration(destinationQueue, () => messageForwarder)));
        }

        [Test]
        public async Task when_a_forwarder_is_started_messages_are_forwarded_via_the_endpoint()
        {
            await CreateSubscriptionEntitiesIfRequired(namespaceConnectionString, TopicName, destinationQueue);
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
        
        private async Task CreateSubscriptionEntitiesIfRequired(string namespaceConnectionString, string sourceTopic, string destinationQueue)
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(namespaceConnectionString);

            if (!await namespaceManager.QueueExistsAsync(destinationQueue))
            {
                var description = new QueueDescription(destinationQueue) {SupportOrdering = false};
                await namespaceManager.CreateQueueAsync(description);
            }

            if (!await namespaceManager.SubscriptionExistsAsync(sourceTopic, destinationQueue))
            {
                var description = new SubscriptionDescription(sourceTopic, destinationQueue) { ForwardTo = destinationQueue };
                await namespaceManager.CreateSubscriptionAsync(description);
            }
        }
    }
}