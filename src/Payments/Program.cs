using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Config;
using NServiceBus.Logging;

namespace Payments
{
    class Program
    {
        static void Main()
        {
            MainAsync().Wait();

            Console.WriteLine("Started...");
            Console.ReadLine();
        }

        private static async Task MainAsync()
        {
            var license = Environment.GetEnvironmentVariable("NServiceBus.License", EnvironmentVariableTarget.User);
            var ordersConnectionString = Environment.GetEnvironmentVariable("Orders.ConnectionString", EnvironmentVariableTarget.User);
            var paymentsConnectionString = Environment.GetEnvironmentVariable("Payments.ConnectionString", EnvironmentVariableTarget.User);

            System.Net.ServicePointManager.DefaultConnectionLimit = Int32.MaxValue;
            
            var defaultFactory = LogManager.Use<DefaultFactory>();
            defaultFactory.Level(LogLevel.Warn);

            var endpointConfig = new EndpointConfiguration("Payments");
            endpointConfig.License(license);
            endpointConfig.UsePersistence<InMemoryPersistence>();
            endpointConfig.SendFailedMessagesTo("error");
            var transport = endpointConfig.UseTransport<AzureServiceBusTransport>();
            transport.ConnectionString(paymentsConnectionString);
            transport.UseForwardingTopology();
            var factories = transport.MessagingFactories();
            var receivers = transport.MessageReceivers();

            var perReceiverConcurrency = 10;
            var numberOfReceivers = 2; // Premium messaging has 2 partitions
            var globalConcurrency = numberOfReceivers * perReceiverConcurrency;

            endpointConfig.LimitMessageProcessingConcurrencyTo(globalConcurrency);
            receivers.PrefetchCount(500);
            factories.NumberOfMessagingFactoriesPerNamespace(numberOfReceivers * 3); //Bus receiver, forwarder sender, bus sender
            transport.NumberOfClientsPerEntity(numberOfReceivers);
            factories.BatchFlushInterval(TimeSpan.FromMilliseconds(100));
           
            var endpoint = await Endpoint.Start(endpointConfig).ConfigureAwait(false);
            var forwarder = new Forwarder(
                ordersConnectionString,
                "Returns",
                "Payments",
                endpoint, m => Type.GetType($"{(string)m.Properties["Asos.EnclosedType"]}, Payments"));

            Task forwarderTask = forwarder.Start();
        }
    }
}