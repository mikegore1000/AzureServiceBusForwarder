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
        private const int NumberOfFactories = 10; // TODO: Make this configurable

        private readonly ForwarderSourceConfiguration sourceConfiguration;
        private readonly ForwarderDestinationConfiguration destinationConfiguration;
        private readonly List<QueueClient> clients = new List<QueueClient>();
        private readonly MessageForwarder messageForwarder;

        public Forwarder(ForwarderSourceConfiguration sourceConfiguration, ForwarderDestinationConfiguration destinationConfiguration, Func<BrokeredMessage, Type> messageMapper, ISerializer serializer)
        {
            Guard.IsNotNull(sourceConfiguration, nameof(sourceConfiguration));
            Guard.IsNotNull(destinationConfiguration, nameof(destinationConfiguration));
            Guard.IsNotNull(messageMapper, nameof(messageMapper));
            Guard.IsNotNull(serializer, nameof(serializer));

            this.sourceConfiguration = sourceConfiguration;
            this.destinationConfiguration = destinationConfiguration;
            this.messageForwarder = new MessageForwarder(destinationConfiguration.DestinationQueue, destinationConfiguration.Endpoint, messageMapper, serializer);
        }

        public void Start()
        {
            CreateQueueClients();
            Poll();
        }

        public async Task CreateSubscriptionEntitiesIfRequired()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(sourceConfiguration.ConnectionString);

            if (!await namespaceManager.QueueExistsAsync(destinationConfiguration.DestinationQueue))
            {
                await namespaceManager.CreateQueueAsync(destinationConfiguration.DestinationQueue);
            }

            if (!await namespaceManager.SubscriptionExistsAsync(sourceConfiguration.TopicName, destinationConfiguration.DestinationQueue))
            {
                var description = new SubscriptionDescription(sourceConfiguration.TopicName, destinationConfiguration.DestinationQueue) { ForwardTo = destinationConfiguration.DestinationQueue };
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
                var messages = await client.ReceiveBatchAsync(sourceConfiguration.ReceiveBatchSize);
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
                var client = QueueClient.CreateFromConnectionString(sourceConfiguration.ConnectionString, destinationConfiguration.DestinationQueue);
                clients.Add(client);
            }
        }
    }
}