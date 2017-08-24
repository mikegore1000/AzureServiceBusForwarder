using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using NServiceBus.AzureServiceBusForwarder.Serializers;

namespace NServiceBus.AzureServiceBusForwarder
{
    public class MessageForwarder
    {
        private static readonly List<string> IgnoredHeaders = new List<string>
        {
            "NServiceBus.Transport.Encoding" // Don't assume endpoint forwarding into uses the same serialization
        };

        private readonly Func<BrokeredMessage, Type> messageMapper;
        private readonly IEndpointInstance endpoint;
        private readonly string destinationQueue;
        private readonly ISerializer serializer;

        public MessageForwarder(string destinationQueue, IEndpointInstance endpoint, Func<BrokeredMessage, Type> messageMapper, ISerializer serializer)
        {
            Guard.IsNotNull(messageMapper, nameof(messageMapper));
            Guard.IsNotNull(endpoint, nameof(endpoint));
            Guard.IsNotNullOrEmpty(destinationQueue, nameof(destinationQueue));
            Guard.IsNotNull(serializer, nameof(serializer));

            this.messageMapper = messageMapper;
            this.endpoint = endpoint;
            this.destinationQueue = destinationQueue;
            this.serializer = serializer;
        }

        public Task FowardMessage(BrokeredMessage message)
        {
            var messageType = messageMapper(message);
            var body = GetMessageBody(messageType, message);
            var sendOptions = new SendOptions();
            sendOptions.SetDestination(destinationQueue);

            foreach (var p in message.Properties.Where(x => !IgnoredHeaders.Contains(x.Key)))
            {
                sendOptions.SetHeader(p.Key, p.Value.ToString());
            }

            return endpoint.Send(body, sendOptions);
        }

        public object GetMessageBody(Type type, BrokeredMessage brokeredMessage)
        {
            return serializer.Deserialize(brokeredMessage, type);
        }
    }
}