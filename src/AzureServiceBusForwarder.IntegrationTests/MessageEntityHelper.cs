using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus;

namespace AzureServiceBusForwarder.IntegrationTests
{
    internal class MessageEntityHelper
    {
        internal static async Task CreateQueue(string queueName)
        {
            var manageClient = CreateNamespaceManager();

            if (await manageClient.QueueExistsAsync(queueName))
            {
                await manageClient.DeleteQueueAsync(queueName);
            }

            await manageClient.CreateQueueAsync(queueName);
        }

        internal static async Task CreateTopicWithSubscription(string topicName)
        {
            var manageClient = CreateNamespaceManager();

            if (await manageClient.TopicExistsAsync(topicName))
            {
                await manageClient.DeleteTopicAsync(topicName);
            }

            await manageClient.CreateTopicAsync(topicName);
            await manageClient.CreateSubscriptionAsync(topicName, topicName);
        }
        
        private static NamespaceManager CreateNamespaceManager()
        {
            var connectionString = Environment.GetEnvironmentVariable("NServiceBus.AzureServiceBusForwarder.ConnectionString", EnvironmentVariableTarget.User);
            var manageClient = NamespaceManager.CreateFromConnectionString(connectionString);
            return manageClient;
        }
    }
}