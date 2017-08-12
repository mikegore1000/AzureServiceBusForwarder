using System;
using System.Threading.Tasks;
using NServiceBus;

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
            System.Net.ServicePointManager.DefaultConnectionLimit = Int32.MaxValue;

            var endpointConfig = new EndpointConfiguration("Payments");
            endpointConfig.UsePersistence<InMemoryPersistence>();
            endpointConfig.SendFailedMessagesTo("error");
            var transport = endpointConfig.UseTransport<AzureServiceBusTransport>();
            transport.ConnectionString("Endpoint=sb://asb-payments.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=LzHVeICN/qV7I2wt/axOXEMGYjGJijomJixaiqafQz8=");
            transport.UseForwardingTopology();
            var routing = transport.NamespaceRouting();

            // Can't use this to customize the subscription filter
            // transport.Subscriptions().DescriptionFactory((topicName, endpointName, settings) => new SubscriptionDescription(topicName, endpointName + "bob"));

            var endpoint = await Endpoint.Start(endpointConfig).ConfigureAwait(false);
            var forwarder = new Forwarder(
                "Endpoint=sb://asb-orders.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=3FCJyb5w6ixTgeUl9Cgb/eoiAP12ewB+BPG5V90sONU=",
                "Returns",
                "Payments",
                endpoint, m => Type.GetType($"{(string)m.Properties["Asos.EnclosedType"]}, Payments"));

            Task forwarderTask = forwarder.Start();
        }
    }
}