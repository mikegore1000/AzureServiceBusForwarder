using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using NServiceBus;

namespace Payments
{
    public class Forwarder
    {
        private const int PrefetchCount = 100; // TODO: Make this configurable
        private const int NumberOfFactories = 10; // TODO: Make this configurable
        private const int MaxConcurrentCalls = 100; // TODO: Make this configurable

        private readonly string sourceConnectionString;
        private readonly string destinationConnectionString;

        private readonly string topicName;
        private readonly string subscriberName;
        private readonly IMessageSession endpoint;
        private readonly Func<BrokeredMessage, Type> messageMapper;
        private readonly List<QueueClient> clients = new List<QueueClient>();
        private QueueClient destinationQueueClient;
        

        private static readonly List<string> IgnoredHeaders = new List<string>
        {
            "NServiceBus.Transport.Encoding" // Don't assume endpoint forwarding into uses the same serialization
        };

        public Forwarder(string sourceConnectionString, string topicName, string destinationConnectionString, string subscriberName, IMessageSession endpoint, Func<BrokeredMessage, Type> messageMapper)
        {
            this.sourceConnectionString = sourceConnectionString;
            this.destinationConnectionString = destinationConnectionString;
            this.topicName = topicName;
            this.subscriberName = subscriberName;
            this.endpoint = endpoint;
            this.messageMapper = messageMapper;
        }

        public async Task Start()
        {
            await CreateSubscriptionIfRequired();
            CreateSourceQueueClients();
            CreateDestinationQueueClient();
            Poll();
        }

        private void Poll()
        {
            foreach (var c in clients)
            {
                c.OnMessageAsync(OnMessage, new OnMessageOptions { MaxConcurrentCalls = MaxConcurrentCalls, AutoComplete = true }); // Can't auto complete if batching!
            }
        }

        private static int MessagesForwarded;

        private async Task OnMessage(BrokeredMessage message)
        {
            // TODO: Optimise performance based on this https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-performance-improvements
            // TODO: Need to look at pulling in batches
            // TODO: Add logging
            var messageType = messageMapper(message);
            var body = GetMessageBody(messageType, message); // TODO: Optimise this, do we even need to convert?
            //var jsonBody = JsonConvert.SerializeObject(body);

            var serializer = new Newtonsoft.Json.JsonSerializer();
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, Encoding.UTF8);
            serializer.Serialize(writer, body);
            writer.Flush();
            stream.Position = 0;

            var destinationMessage = new BrokeredMessage(stream, true);

            // foreach (var p in message.Properties.Where(x => !IgnoredHeaders.Contains(x.Key)))
            foreach (var p in message.Properties) // TODO: Add in expected headers
            {
                destinationMessage.Properties.Add(p.Key, p.Value);
            }

            await destinationQueueClient.SendAsync(destinationMessage);

            var forwarded = Interlocked.Increment(ref MessagesForwarded);
            if (forwarded % 100 == 0)
            {
                Console.WriteLine($"Forwarded {forwarded} messages");
            }
        }

        private void CreateSourceQueueClients()
        {
            for (int i = 0; i < NumberOfFactories; i++)
            {
                var client = QueueClient.CreateFromConnectionString(sourceConnectionString, subscriberName);
                client.PrefetchCount = PrefetchCount;

                clients.Add(client);
            }
        }

        private void CreateDestinationQueueClient()
        {
            destinationQueueClient = QueueClient.CreateFromConnectionString(destinationConnectionString, subscriberName);
        }

        public object GetMessageBody(Type type, BrokeredMessage brokeredMessage)
        {
            return JsonConvert.DeserializeObject(brokeredMessage.GetBody<string>(), type);
        }

        private async Task CreateSubscriptionIfRequired()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(sourceConnectionString);

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