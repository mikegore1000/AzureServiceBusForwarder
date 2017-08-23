using System.IO;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using NUnit.Framework;
using Serializer = NServiceBus.AzureServiceBusForwarder.Serializers;
using Newtonsoft.Json;

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
        public void when_the_content_type_is_json_the_message_can_be_handled()
        {
            var message = new BrokeredMessage { ContentType = "application/json" };
            Assert.That(serializer.CanDeserialize(message), Is.True);
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
            
            var message = new BrokeredMessage(messageStream) { ContentType = "application/json" };

            var result = serializer.Deserialize(message, typeof(TestMessage));

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<TestMessage>());

        }
    }
}
