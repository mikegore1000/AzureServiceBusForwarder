using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace AzureServiceBusForwarder
{
    public class TopicMessageForwarder : AzureServiceBusMessageForwarder
    {
        private readonly TopicClient client;
        
        public TopicMessageForwarder(TopicClient client, Action<BrokeredMessage> outgoingMessageMutator) : base(outgoingMessageMutator)
        {
            Guard.IsNotNull(client, nameof(client));
            this.client = client;
        }

        protected override async Task ForwardMessagesToDestination(List<BrokeredMessage> messagesToForward)
        {
            await client.SendBatchAsync(messagesToForward);
        }
    }
}