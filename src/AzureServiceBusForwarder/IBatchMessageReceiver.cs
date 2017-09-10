using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace AzureServiceBusForwarder
{
    public interface IBatchMessageReceiver
    {
        Task<IEnumerable<BrokeredMessage>> ReceieveMessages(int batchSize);
        Task CompleteMessages(Guid[] lockTokens);
    }
}