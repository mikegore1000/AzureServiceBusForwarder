using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
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

        private readonly string connectionString;
        private readonly string topicName;
        private readonly string destinationQueue;
        private readonly IEndpointInstance endpoint;
        private readonly Func<BrokeredMessage, Type> messageMapper;
        private readonly List<QueueClient> clients = new List<QueueClient>();

        private ISerializer serializer;
        

        private static readonly List<string> IgnoredHeaders = new List<string>
        {
            "NServiceBus.Transport.Encoding" // Don't assume endpoint forwarding into uses the same serialization
        };

        public Forwarder(string connectionString, string topicName, string destinationQueue, IEndpointInstance endpoint, Func<BrokeredMessage, Type> messageMapper)
        {
            Guard.IsNotNullOrEmpty(connectionString, nameof(connectionString));
            Guard.IsNotNullOrEmpty(topicName, nameof(topicName));
            Guard.IsNotNullOrEmpty(destinationQueue, nameof(destinationQueue));
            Guard.IsNotNull(endpoint, nameof(endpoint));
            Guard.IsNotNull(messageMapper, nameof(messageMapper));

            this.connectionString = connectionString;
            this.topicName = topicName;
            this.destinationQueue = destinationQueue;
            this.endpoint = endpoint;
            this.messageMapper = messageMapper;
        }

        public async Task Start()
        {
            await CreateSubscriptionIfRequired();
            CreateQueueClients();
            Poll();
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
                    var messageType = messageMapper(message);
                    var body = GetMessageBody(messageType, message);
                    var sendOptions = new SendOptions();
                    sendOptions.SetDestination(destinationQueue);

                    foreach (var p in message.Properties.Where(x => !IgnoredHeaders.Contains(x.Key)))
                    {
                        sendOptions.SetHeader(p.Key, p.Value.ToString());
                    }

                    sendTasks.Add(endpoint.Send(body, sendOptions));
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
                var client = QueueClient.CreateFromConnectionString(connectionString, destinationQueue);
                client.PrefetchCount = PrefetchCount;
                clients.Add(client);
            }
        }

        public object GetMessageBody(Type type, BrokeredMessage brokeredMessage)
        {
            var stream = brokeredMessage.GetBody<Stream>();
            var dataContractSerializer = new DataContractSerializer(type);
            using (var reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max))
            {
                return dataContractSerializer.ReadObject(reader);
            }
        }

        public void SetSerializer(ISerializer toUse)
        {
            Guard.IsNotNull(toUse, nameof(toUse));
            serializer = toUse;
        }

        private async Task CreateSubscriptionIfRequired()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            if (!await namespaceManager.QueueExistsAsync(destinationQueue))
            {
                await namespaceManager.CreateQueueAsync(destinationQueue);
            }

            if (!await namespaceManager.SubscriptionExistsAsync(topicName, destinationQueue))
            {
                var description = new SubscriptionDescription(topicName, destinationQueue) { ForwardTo = destinationQueue };
                await namespaceManager.CreateSubscriptionAsync(description);
            }
        }
    }
}