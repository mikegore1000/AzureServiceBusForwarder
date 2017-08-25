using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using NServiceBus.AzureServiceBusForwarder.Serializers;

namespace NServiceBus.AzureServiceBusForwarder
{
    public class Forwarder
    {
        private const int PrefetchCount = 100; // TODO: Make this configurable
        private const int NumberOfFactories = 10; // TODO: Make this configurable
        private const int ReceiveBatchSize = 500;  // TODO: Make this configurable

        private readonly ForwarderSourceConfiguration sourceConfiguration;
        private readonly string destinationQueue;
        private readonly List<QueueClient> clients = new List<QueueClient>();
        private readonly MessageForwarder messageForwarder;

        public Forwarder(ForwarderSourceConfiguration sourceConfiguration, string destinationQueue, IEndpointInstance endpoint, Func<BrokeredMessage, Type> messageMapper, ISerializer serializer)
        {
            Guard.IsNotNullOrEmpty(destinationQueue, nameof(destinationQueue));
            Guard.IsNotNull(endpoint, nameof(endpoint));
            Guard.IsNotNull(messageMapper, nameof(messageMapper));
            Guard.IsNotNull(serializer, nameof(serializer));

            this.sourceConfiguration = sourceConfiguration;
            this.destinationQueue = destinationQueue;
            this.messageForwarder = new MessageForwarder(destinationQueue, endpoint, messageMapper, serializer);
        }

        public void Start()
        {
            CreateQueueClients();
            Poll();
        }

        public async Task CreateSubscriptionEntitiesIfRequired()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(sourceConfiguration.ConnectionString);

            if (!await namespaceManager.QueueExistsAsync(destinationQueue))
            {
                await namespaceManager.CreateQueueAsync(destinationQueue);
            }

            if (!await namespaceManager.SubscriptionExistsAsync(sourceConfiguration.TopicName, destinationQueue))
            {
                var description = new SubscriptionDescription(sourceConfiguration.TopicName, destinationQueue) { ForwardTo = destinationQueue };
                await namespaceManager.CreateSubscriptionAsync(description);
            }
        }

        private void Poll()
        {
            foreach (var c in clients)
            {
                PollClient(c);
            }
        }

        private async Task PollClient(QueueClient client) // TODO: Support cancellation
        {
            while (true)
            {
                var messages = await client.ReceiveBatchAsync(ReceiveBatchSize); // TODO: Make configurable
                var sentMessageTokens = new List<Guid>();
                var sendTasks = new List<Task>();

                foreach (var message in messages)
                {
                    sendTasks.Add(messageForwarder.ForwardMessage(message));
                    sentMessageTokens.Add(message.LockToken);
                }

                await Task.WhenAll(sendTasks).ConfigureAwait(false);
                await client.CompleteBatchAsync(sentMessageTokens);
            }
        }

        private void CreateQueueClients()
        {
            for (int i = 0; i < NumberOfFactories; i++)
            {
                var client = QueueClient.CreateFromConnectionString(sourceConfiguration.ConnectionString, destinationQueue);
                client.PrefetchCount = PrefetchCount;
                clients.Add(client);
            }
        }
    }
}