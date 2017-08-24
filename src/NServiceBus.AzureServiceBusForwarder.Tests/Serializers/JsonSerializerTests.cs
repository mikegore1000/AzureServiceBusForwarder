using System.IO;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using NUnit.Framework;
using Serializer = NServiceBus.AzureServiceBusForwarder.Serializers;
using static NServiceBus.AzureServiceBusForwarder.Tests.MessageFactory;

namespace NServiceBus.AzureServiceBusForwarder.Tests.Serializers
{
    [TestFixture]
    public class JsonSerializerTests
    {
        private Serializer.JsonSerializer serializer;

        [SetUp]
        public void Setup()
        {
            serializer = new Serializer.JsonSerializer();
        }

        [Test]
        public async Task when_a_json_message_is_provided_it_can_be_deserialized()
        {
            var jsonSerializer = new Newtonsoft.Json.JsonSerializer();
            var messageStream = new MemoryStream();
            var writer = new StreamWriter(messageStream);
            var body = new TestMessage();

            jsonSerializer.Serialize(writer, body);
            await writer.FlushAsync();
            messageStream.Position = 0;
            var message = await CreateMessageWithJsonBody();

            var result = serializer.Deserialize(message, typeof(TestMessage));

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<TestMessage>());
        }
    }
}
