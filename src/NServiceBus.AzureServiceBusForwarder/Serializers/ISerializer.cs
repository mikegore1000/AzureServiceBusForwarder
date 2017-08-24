using System;
using Microsoft.ServiceBus.Messaging;

namespace NServiceBus.AzureServiceBusForwarder.Serializers
{
    public interface ISerializer
    {
        object Deserialize(BrokeredMessage message, Type type);
    }
}