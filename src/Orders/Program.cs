using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Orders.Events;

namespace Orders
{
    class Program
    {
        private const string TopicName = "Returns";

        private static string connectionString;
        private static TopicClient topicClient;

        static void Main()
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = Int32.MaxValue;

            connectionString = Environment.GetEnvironmentVariable("Orders.ConnectionString", EnvironmentVariableTarget.User);
            topicClient = TopicClient.CreateFromConnectionString(connectionString, TopicName);

            MainAsync().Wait();

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        private static async Task MainAsync()
        {
            await CreateTopic();

            while(true)
            {
                int messageCount;
                Console.Write("Enter a number of messages to send or enter anything else to quit: ");

                if (!int.TryParse(Console.ReadLine(), out messageCount))
                {
                    return;
                }

                int totalMessages = messageCount;
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                do
                {
                    var blockSize = messageCount >= 1000 ? 1000 : messageCount;
                    await SendMessages(blockSize);
                    messageCount -= blockSize;
                } while (messageCount > 0);                

                stopWatch.Stop();
                Console.WriteLine($"Took {stopWatch.Elapsed} to send {totalMessages}");
            }
        }

        private static async Task SendMessages(int messageCount)
        {
            var messages = new List<BrokeredMessage>();

            for (int i = 0; i < messageCount; i++)
            {
                messages.Add(await CreateMessage());
            }

            await topicClient.SendBatchAsync(messages);

            Console.WriteLine($"Sent block of {messageCount} messages");
        }

        private static async Task<BrokeredMessage> CreateMessage()
        {
            var jsonSerializer = new Newtonsoft.Json.JsonSerializer();
            var messageStream = new MemoryStream();
            var writer = new StreamWriter(messageStream);
            var body = new OrderAccepted { OrderReference = Guid.NewGuid() };

            jsonSerializer.Serialize(writer, body);
            await writer.FlushAsync();
            messageStream.Position = 0;

            var message =  new BrokeredMessage(messageStream) { ContentType = "application/json" };
            message.Properties["Asos.EnclosedType"] = "Orders.Events.OrderAccepted";
            return message;
        }

        private static async Task CreateTopic()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            if (!await namespaceManager.TopicExistsAsync(TopicName))
            {
                await namespaceManager.CreateTopicAsync(new TopicDescription(TopicName) { SupportOrdering = false });
            }
        }
    }
}
