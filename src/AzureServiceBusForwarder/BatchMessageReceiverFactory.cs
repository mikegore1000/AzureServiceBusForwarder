using Microsoft.ServiceBus.Messaging;

namespace AzureServiceBusForwarder
{
    internal class BatchMessageReceiverFactory
    {
        public BatchMessageReceiver Create(QueueClient client)
        {
            return new BatchMessageReceiver(client);
        }
    }
}