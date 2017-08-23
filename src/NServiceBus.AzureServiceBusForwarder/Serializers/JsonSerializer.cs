using System;
using System.IO;
using Microsoft.ServiceBus.Messaging;

namespace NServiceBus.AzureServiceBusForwarder.Serializers
{
    public class JsonSerializer
    {
        private readonly Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

        public bool CanDeserialize(BrokeredMessage message)
        {
            return message.ContentType == "application/json";
        }

        public object Deserialize(BrokeredMessage message, Type type)
        {
            using (var reader = new StreamReader(message.GetBody<Stream>()))
            {
                return serializer.Deserialize(reader, type);
            }
        }
    }
}
