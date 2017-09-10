using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace AzureServiceBusForwarder
{
    public class SubscriptionBatchMessageReceiver : IBatchMessageReceiver
    {
        private readonly SubscriptionClient client;

        public SubscriptionBatchMessageReceiver(SubscriptionClient client)
        {
            Guard.IsNotNull(client, nameof(client));
            this.client = client;
        }

        public Task<IEnumerable<BrokeredMessage>> ReceieveMessages(int batchSize)
        {
            return client.ReceiveBatchAsync(batchSize);
        }

        public Task CompleteMessages(Guid[] lockTokens)
        {
            if (lockTokens.Any())
            {
                return client.CompleteBatchAsync(lockTokens);
            }

            return Task.CompletedTask;
        }
    }
}