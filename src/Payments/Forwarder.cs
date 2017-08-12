using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using NServiceBus;

namespace Payments
{
    public class Forwarder
    {
        private const int PrefetchCount = 100; // TODO: Make this configurable
        private const int NumberOfFactories = 10; // TODO: Make this configurable
        private const int MaxConcurrentCalls = 100; // TODO: Make this configurable

        private readonly string connectionString;
        private readonly string topicName;
        private readonly string subscriberName;
        private readonly IEndpointInstance endpoint;
        private readonly Func<BrokeredMessage, Type> messageMapper;
        private readonly List<QueueClient> clients = new List<QueueClient>();
        

        private static readonly List<string> IgnoredHeaders = new List<string>
        {
            "NServiceBus.Transport.Encoding" // Don't assume endpoint forwarding into uses the same serialization
        };

        public Forwarder(string connectionString, string topicName, string subscriberName, IEndpointInstance endpoint, Func<BrokeredMessage, Type> messageMapper)
        {
            this.connectionString = connectionString;
            this.topicName = topicName;
            this.subscriberName = subscriberName;
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
                c.OnMessageAsync(OnMessage, new OnMessageOptions { MaxConcurrentCalls = MaxConcurrentCalls });
            }
        }

        private async Task OnMessage(BrokeredMessage message)
        {
            // TODO: Optimise performance based on this https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-performance-improvements
            // TODO: Need to look at pulling in batches
            // TODO: Add logging
            var messageType = messageMapper(message);
            var body = GetMessageBody(messageType, message);
            var sendOptions = new SendOptions();
            sendOptions.SetDestination(subscriberName);

            foreach (var p in message.Properties.Where(x => !IgnoredHeaders.Contains(x.Key)))
            {
                sendOptions.SetHeader(p.Key, p.Value.ToString());
            }

            await endpoint.Send(body, sendOptions);
        }

        private void CreateQueueClients()
        {
            for (int i = 0; i < NumberOfFactories; i++)
            {
                var client = QueueClient.CreateFromConnectionString(connectionString, subscriberName);
                client.PrefetchCount = PrefetchCount;

                clients.Add(client);
            }
        }

        public object GetMessageBody(Type type, BrokeredMessage brokeredMessage)
        {
            var stream = brokeredMessage.GetBody<Stream>();
            var serializer = new DataContractSerializer(type);
            using (var reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max))
            {
                return serializer.ReadObject(reader);
            }
        }

        private async Task CreateSubscriptionIfRequired()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            if (!await namespaceManager.QueueExistsAsync(subscriberName))
            {
                await namespaceManager.CreateQueueAsync(subscriberName);
            }

            if (!await namespaceManager.SubscriptionExistsAsync(topicName, subscriberName))
            {
                var description = new SubscriptionDescription(topicName, subscriberName) { ForwardTo = subscriberName };
                await namespaceManager.CreateSubscriptionAsync(description);
            }
        }
    }
}