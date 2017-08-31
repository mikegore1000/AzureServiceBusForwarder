using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace AzureServiceBusForwarder
{
    public interface IMessageForwarder
    {
        Task<IEnumerable<Guid>> ForwardMessages(IEnumerable<BrokeredMessage> messages);
    }
}