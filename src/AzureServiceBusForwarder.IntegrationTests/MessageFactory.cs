using System.IO;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace AzureServiceBusForwarder.IntegrationTests
{
    public class MessageFactory
    {
        public static async Task<BrokeredMessage> CreateMessageWithJsonBody()
        {
            var jsonSerializer = new Newtonsoft.Json.JsonSerializer();
            var messageStream = new MemoryStream();
            var writer = new StreamWriter(messageStream);
            var body = new TestMessage();

            jsonSerializer.Serialize(writer, body);
            await writer.FlushAsync();
            messageStream.Position = 0;

            return new BrokeredMessage(messageStream) {ContentType = "application/json"};
        }
    }
}