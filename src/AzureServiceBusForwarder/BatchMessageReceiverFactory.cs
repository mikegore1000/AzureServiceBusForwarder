using Microsoft.ServiceBus.Messaging;

namespace AzureServiceBusForwarder
{
    internal class BatchMessageReceiverFactory
    {
        public QueueBatchMessageReceiver Create(QueueClient client)
        {
            return new QueueBatchMessageReceiver(client);
        }
    }
}