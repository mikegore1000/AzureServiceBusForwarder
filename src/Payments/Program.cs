using System;
using System.Threading.Tasks;
using NServiceBus;
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