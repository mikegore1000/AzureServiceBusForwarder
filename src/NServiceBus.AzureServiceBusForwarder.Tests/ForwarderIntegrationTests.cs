using System;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using NUnit.Framework;
using static NServiceBus.AzureServiceBusForwarder.Tests.MessageFactory;

namespace NServiceBus.AzureServiceBusForwarder.Tests
{
    public class ForwarderIntegrationTests
    {
        private IEndpointInstance endpointFake;

        [SetUp]
        public void Setup()
        {
            endpointFake = A.Fake<IEndpointInstance>();
        }

        [Test]
        public async Task when_a_forwarder_is_started_messages_are_forwarded_via_the_endpoint()
        {
            const string topicName = "sourceTopic";
            var namespaceConnectionString = Environment.GetEnvironmentVariable("NServiceBus.AzureServiceBusForwarder.ConnectionString", EnvironmentVariableTarget.User);
            var namespaceManager = NamespaceManager.CreateFromConnectionString(namespaceConnectionString);

            if (await namespaceManager.TopicExistsAsync(topicName))
            {
                await namespaceManager.DeleteTopicAsync(topicName);
            }

            await namespaceManager.CreateTopicAsync(topicName);

            var forwarder = new Forwarder(namespaceConnectionString, topicName, "destinationQueue", endpointFake, message => typeof(TestMessage), new AzureServiceBusForwarder.Serializers.JsonSerializer());
            await forwarder.Start();

            var topicClient = TopicClient.CreateFromConnectionString(namespaceConnectionString, topicName);
            var eventMessage = await CreateMessageWithJsonBody();
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