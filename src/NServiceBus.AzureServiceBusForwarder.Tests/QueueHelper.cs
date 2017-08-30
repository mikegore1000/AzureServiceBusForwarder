using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus;

namespace NServiceBus.AzureServiceBusForwarder.Tests
{
    internal class QueueHelper
    {
        internal static async Task CreateQueue(string queueName)
        {
            var connectionString = Environment.GetEnvironmentVariable("NServiceBus.AzureServiceBusForwarder.ConnectionString", EnvironmentVariableTarget.User);
            var manageClient = NamespaceManager.CreateFromConnectionString(connectionString);

            if (await manageClient.QueueExistsAsync(queueName))
            {
                await manageClient.DeleteQueueAsync(queueName);
            }

            await manageClient.CreateQueueAsync(queueName);
        }
    }
}