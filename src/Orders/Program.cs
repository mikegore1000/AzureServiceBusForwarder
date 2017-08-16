using System;
using System.Collections.Generic;
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

                
                await SendMessages(messageCount);
            }
        }

        private static async Task SendMessages(int messageCount)
        {
            var messages = new List<BrokeredMessage>();

            for (int i = 0; i < messageCount; i++)
            {
                var message = new BrokeredMessage(new OrderAccepted {OrderReference = Guid.NewGuid()});
                message.Properties["Asos.EnclosedType"] = "Orders.Events.OrderAccepted";
                // Can get rid of these properties as we can bridge on Asos.EnclosedType, but it still works with these headers in place for NSB <-> NSB integration
                message.Properties["NServiceBus.EnclosedMessageTypes"] = "Orders.Events.OrderAccepted";
                message.Properties["NServiceBus.Transport.Encoding"] = "application/octect-stream";

                messages.Add(message);
            }

            await topicClient.SendBatchAsync(messages);
        }

        private static async Task CreateTopic()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            if (!await namespaceManager.TopicExistsAsync(TopicName))
            {
                await namespaceManager.CreateTopicAsync(new TopicDescription(TopicName));
            }
        }
    }
}
