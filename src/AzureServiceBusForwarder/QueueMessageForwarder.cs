using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace AzureServiceBusForwarder
{
    public class QueueMessageForwarder : AzureServiceBusMessageForwarder
    {
        protected readonly QueueClient sendClient;
        
        public QueueMessageForwarder(QueueClient sendClient, Action<BrokeredMessage> outgoingMessageMutator) : base(outgoingMessageMutator)
        {
            Guard.IsNotNull(sendClient, nameof(sendClient));

            this.sendClient = sendClient;
        }

        protected override async Task ForwardMessagesToDestination(List<BrokeredMessage> messagesToForward)
        {
            await sendClient.SendBatchAsync(messagesToForward);
        }
    }
}