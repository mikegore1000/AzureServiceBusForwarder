using Microsoft.ServiceBus.Messaging;

namespace NServiceBus.AzureServiceBusForwarder
{
    internal class BatchMessageReceiverFactory
    {
        public BatchMessageReceiver Create(QueueClient client)
        {
            return new BatchMessageReceiver(client);
        }
    }
}