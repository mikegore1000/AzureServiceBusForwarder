using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace AzureServiceBusForwarder
{
    public class QueueMessageForwarder : AzureServiceBusMessageForwarder
    {
        public QueueMessageForwarder(QueueClient sendClient, Action<BrokeredMessage> outgoingMessageMutator) : base(sendClient, outgoingMessageMutator)
        {
        }

        protected override async Task ForwardMessagesToDestination(List<BrokeredMessage> messagesToForward)
        {
            await sendClient.SendBatchAsync(messagesToForward);
        }
    }
}