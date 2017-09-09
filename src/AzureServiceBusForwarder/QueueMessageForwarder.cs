using System;
using Microsoft.ServiceBus.Messaging;

namespace AzureServiceBusForwarder
{
    public class QueueMessageForwarder : AzureServiceBusMessageForwarder
    {
        public QueueMessageForwarder(QueueClient sendClient, Action<BrokeredMessage> outgoingMessageMutator) : base(sendClient, outgoingMessageMutator)
        {
        }
    }
}