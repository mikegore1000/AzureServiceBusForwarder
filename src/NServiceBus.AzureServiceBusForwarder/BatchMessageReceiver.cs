using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace NServiceBus.AzureServiceBusForwarder
{
    public class BatchMessageReceiver
    {
        private readonly QueueClient client;

        public BatchMessageReceiver(QueueClient client)
        {
            Guard.IsNotNull(client, nameof(client));
            this.client = client;
        }

        public Task<IEnumerable<BrokeredMessage>> ReceieveMessages(int batchSize)
        {
            return client.ReceiveBatchAsync(batchSize);
        }

        public Task CompleteMessages(IEnumerable<Guid> lockTokens)
        {
            return client.CompleteBatchAsync(lockTokens);
        }
    }
}