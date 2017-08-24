using System;
using Microsoft.ServiceBus.Messaging;

namespace NServiceBus.AzureServiceBusForwarder.Serializers
{
    public interface ISerializer
    {
        bool CanDeserialize(BrokeredMessage message);
        object Deserialize(BrokeredMessage message, Type type);
    }
}