using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace AzureServiceBusForwarder
{
    public class AzureServiceBusMessageForwarder : IMessageForwarder
    {
        private static readonly Action<BrokeredMessage> DefaultMessageMutator = (message) => { };

        private readonly QueueClient sendClient;
        private readonly Action<BrokeredMessage> outgoingMessageMutator;

        public AzureServiceBusMessageForwarder(QueueClient sendClient, Action<BrokeredMessage> outgoingMessageMutator)
        {
            Guard.IsNotNull(sendClient, nameof(sendClient));

            this.sendClient = sendClient;
            this.outgoingMessageMutator = outgoingMessageMutator ?? DefaultMessageMutator;
        }

        public async Task<IEnumerable<Guid>> ForwardMessages(IEnumerable<BrokeredMessage> messages)
        {
            var lockTokens = new List<Guid>();
            var messagesToForward = new List<BrokeredMessage>();

            foreach (var message in messages)
            {
                var forwardMessage = new BrokeredMessage(message.GetBody<Stream>());
                CopyHeaders(message, forwardMessage);
                outgoingMessageMutator(forwardMessage);

                messagesToForward.Add(forwardMessage);
                lockTokens.Add(message.LockToken);
            }

            if (messagesToForward.Any())
            {
                await sendClient.SendBatchAsync(messagesToForward);
            }
            return lockTokens;
        }

        public void CopyHeaders(BrokeredMessage from, BrokeredMessage to)
        {
            to.MessageId = from.MessageId;
            to.ContentType = from.ContentType;

            foreach (var property in from.Properties)
            {
                to.Properties[property.Key] = property.Value;
            }
        }
    }
}