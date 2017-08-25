﻿using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.AzureServiceBusForwarder;
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
            defaultFactory.Level(LogLevel.Info);

            var endpointConfig = new EndpointConfiguration("Payments");
            if (!string.IsNullOrEmpty(license))
            {
                endpointConfig.License(license);
            }
            endpointConfig.UsePersistence<InMemoryPersistence>();
            endpointConfig.SendFailedMessagesTo("error");
            var transport = endpointConfig.UseTransport<AzureServiceBusTransport>();
            transport.ConnectionString(paymentsConnectionString);
            var topology = transport.UseForwardingTopology();
            topology.NumberOfEntitiesInBundle(1);
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
            transport.Transactions(TransportTransactionMode.ReceiveOnly); // Use peek lock

            var endpoint = await Endpoint.Start(endpointConfig).ConfigureAwait(false);
            var forwarder = new Forwarder(
                new ForwarderSourceConfiguration(ordersConnectionString, "Returns"),
                new ForwarderDestinationConfiguration("Payments", endpoint),
                m => Type.GetType($"{(string)m.Properties["Asos.EnclosedType"]}, Payments"),
                new NServiceBus.AzureServiceBusForwarder.Serializers.JsonSerializer());

            await forwarder.CreateSubscriptionEntitiesIfRequired();
            forwarder.Start();
        }
    }
}