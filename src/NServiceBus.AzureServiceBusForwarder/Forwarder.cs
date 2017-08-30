using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using NServiceBus.AzureServiceBusForwarder.Serializers;
using NServiceBus.Logging;

namespace NServiceBus.AzureServiceBusForwarder
{
    public class Forwarder
    {
        private const int NumberOfFactories = 10; // TODO: Make this configurable

        private readonly ForwarderSourceConfiguration sourceConfiguration;
        private readonly ForwarderDestinationConfiguration destinationConfiguration;
        private readonly ILog logger;
        private readonly List<BatchMessageReceiver> messageReceivers = new List<BatchMessageReceiver>();
        private readonly MessageForwarder messageForwarder;
        private readonly BatchMessageReceiverFactory batchMessageReceiverFactory;

        public Forwarder(ForwarderSourceConfiguration sourceConfiguration, ForwarderDestinationConfiguration destinationConfiguration, Func<BrokeredMessage, Type> messageMapper, ISerializer serializer, ILog logger)
        {
            Guard.IsNotNull(sourceConfiguration, nameof(sourceConfiguration));
            Guard.IsNotNull(destinationConfiguration, nameof(destinationConfiguration));
            Guard.IsNotNull(messageMapper, nameof(messageMapper));
            Guard.IsNotNull(serializer, nameof(serializer));
            Guard.IsNotNull(logger, nameof(logger));

            this.sourceConfiguration = sourceConfiguration;
            this.destinationConfiguration = destinationConfiguration;
            this.logger = logger;
            this.messageForwarder = new MessageForwarder(destinationConfiguration.DestinationQueue, destinationConfiguration.Endpoint, messageMapper, serializer);
            this.batchMessageReceiverFactory = new BatchMessageReceiverFactory();
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
                var description = new QueueDescription(destinationConfiguration.DestinationQueue) {SupportOrdering = false};
                await namespaceManager.CreateQueueAsync(description);
            }

            if (!await namespaceManager.SubscriptionExistsAsync(sourceConfiguration.TopicName, destinationConfiguration.DestinationQueue))
            {
                var description = new SubscriptionDescription(sourceConfiguration.TopicName, destinationConfiguration.DestinationQueue) { ForwardTo = destinationConfiguration.DestinationQueue };
                await namespaceManager.CreateSubscriptionAsync(description);
            }
        }

        private void Poll()
        {
            foreach (var messageReceiver in messageReceivers)
            {
                PollMessageReceiever(messageReceiver);
            }
        }

        private async Task PollMessageReceiever(BatchMessageReceiver receiver) // TODO: Support cancellation
        {
            while (true)
            {
                var messages = await receiver.ReceieveMessages(sourceConfiguration.ReceiveBatchSize);
                var sentMessageTokens = await messageForwarder.ForwardMessages(messages);
                await receiver.CompleteMessages(sentMessageTokens.ToArray());
            }
        }

        private void CreateQueueClients()
        {
            for (int i = 0; i < NumberOfFactories; i++)
            {
                var client = QueueClient.CreateFromConnectionString(sourceConfiguration.ConnectionString, destinationConfiguration.DestinationQueue);
                client.PrefetchCount = sourceConfiguration.PrefetchCount;
                messageReceivers.Add(batchMessageReceiverFactory.Create(client));
            }
        }
    }
}