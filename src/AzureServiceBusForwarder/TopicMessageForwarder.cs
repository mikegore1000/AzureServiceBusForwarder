using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace AzureServiceBusForwarder
{
    public class TopicMessageForwarder : AzureServiceBusMessageForwarder
    {
        public TopicMessageForwarder(TopicClient client, Action<BrokeredMessage> outgoingMessageMutator) : base(outgoingMessageMutator)
        {
            Guard.IsNotNull(client, nameof(client));
        }

        protected override Task ForwardMessagesToDestination(List<BrokeredMessage> messagesToForward)
        {
            throw new NotImplementedException();
        }
    }
}