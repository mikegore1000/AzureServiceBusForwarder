using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureServiceBusForwarder;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Transport.AzureServiceBus;

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
            var instrumentationKey = Environment.GetEnvironmentVariable("Payments.InstrumentationKey", EnvironmentVariableTarget.User);
            var license = Environment.GetEnvironmentVariable("NServiceBus.License", EnvironmentVariableTarget.User);
            var ordersConnectionString = Environment.GetEnvironmentVariable("Orders.ConnectionString", EnvironmentVariableTarget.User);
            var paymentsConnectionString = Environment.GetEnvironmentVariable("Payments.ConnectionString", EnvironmentVariableTarget.User);

            System.Net.ServicePointManager.DefaultConnectionLimit = Int32.MaxValue;

            TelemetryConfiguration.Active.InstrumentationKey = instrumentationKey;
            var telemetryClient = new TelemetryClient();
            
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
            var factories = transport.MessagingFactories();
            var receivers = transport.MessageReceivers();

            var perReceiverConcurrency = 10;
            var numberOfReceivers = 2; // Premium messaging has 2 partitions
            var globalConcurrency = numberOfReceivers * perReceiverConcurrency;
            
            endpointConfig.LimitMessageProcessingConcurrencyTo(globalConcurrency);
            factories.NumberOfMessagingFactoriesPerNamespace(numberOfReceivers * 2); //Bus receiver, bus sender
            transport.NumberOfClientsPerEntity(numberOfReceivers);
            factories.BatchFlushInterval(TimeSpan.FromMilliseconds(100));

            receivers.PrefetchCount(1000); // The more we prefetch, the better the throughput will be, needs to be balanced though as you can only pull so many messages per batch
            topology.NumberOfEntitiesInBundle(1); // Only use 1 bundle topic, there is no benefit to using more and Particular are going to remove this support moving forward
            transport.Transactions(TransportTransactionMode.ReceiveOnly); // Use peek lock, vastly quicker than 
            transport.BrokeredMessageBodyType(SupportedBrokeredMessageBodyTypes.Stream); // Need to use this for non-NSB integrations - will be what Particular use moving forward too
            transport.TransportType(TransportType.Amqp); // Use this rather than netmessaging, allows more connections to the namespace and is an open standard

            // See here for a better example of this that handles ungraceful shutdowns - https://github.com/Particular/docs.particular.net/blob/master/samples/application-insights/Metrics_1/Endpoint/ApplicationInsightsFeature.cs
            var metricOptions = endpointConfig.EnableMetrics();
            metricOptions.RegisterObservers(context =>
            {
                foreach (var duration in context.Durations)
                {
                    duration.Register(observer: durationLength => telemetryClient.TrackMetric(new MetricTelemetry(duration.Name, durationLength.TotalMilliseconds)));
                }

                foreach (var signal in context.Signals)
                {
                    signal.Register(observer: () => telemetryClient.TrackEvent(new EventTelemetry(signal.Name)));
                }
            });

            var endpoint = await Endpoint.Start(endpointConfig).ConfigureAwait(false);

            var forwarderConfig = new ForwarderConfiguration(
                    new ForwarderSourceConfiguration(ordersConnectionString, "Returns", 500, () => CreateMessageReceiver(ordersConnectionString, "Payments")),
                    new ForwarderDestinationConfiguration("Payments", () => CreateMessageForwarder(paymentsConnectionString, "Payments")))
                .UsingLogger(new Logger(LogManager.GetLogger<Forwarder>()))
                .WithConcurrencyOf(3);

            var forwarder = new Forwarder(forwarderConfig);
            await CreateSubscriptionEntitiesIfRequired(ordersConnectionString, "Returns", "Payments");
            forwarder.Start();
        }
        
        private static async Task CreateSubscriptionEntitiesIfRequired(string ordersConnectionString, string sourceTopic, string destinationQueue)
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(ordersConnectionString);

            if (!await namespaceManager.QueueExistsAsync(destinationQueue))
            {
                var description = new QueueDescription(destinationQueue) {SupportOrdering = false};
                await namespaceManager.CreateQueueAsync(description);
            }

            if (!await namespaceManager.SubscriptionExistsAsync(sourceTopic, destinationQueue))
            {
                var description = new SubscriptionDescription(sourceTopic, destinationQueue) { ForwardTo = destinationQueue };
                await namespaceManager.CreateSubscriptionAsync(description);
            }
        }

        private static readonly Dictionary<string, string> messageTypeMapper = new Dictionary<string, string>()
        {
            {"Orders.Events.OrderAccepted", "Orders.Events.OrderAccepted, Payments"}
        };

        private static IBatchMessageReceiver CreateMessageReceiver(string ordersConnectionString, string sourceQueue)
        {
            return new QueueBatchMessageReceiver(
                QueueClient.CreateFromConnectionString(ordersConnectionString, sourceQueue));
        }

        private static IMessageForwarder CreateMessageForwarder(string paymentsConnectionString, string destinationQueue)
        {
            return new QueueMessageForwarder(
                QueueClient.CreateFromConnectionString(paymentsConnectionString, destinationQueue),
                (message) =>
                {
                    message.Properties["NServiceBus.EnclosedMessageTypes"] = messageTypeMapper[message.Properties["Asos.EnclosedType"].ToString()];
                    message.Properties["NServiceBus.MessageIntent"] = "Publish";
                    message.Properties["NServiceBus.Transport.Encoding"] = "application/octect-stream";
                    message.Properties["NServiceBus.TimeSent"] = DateTimeExtensions.ToWireFormattedString(DateTime.UtcNow);
                    message.Properties["NServiceBus.MessageId"] = message.MessageId;
                });
        }

        private class Logger : ILogger
        {
            private readonly ILog logger;

            public Logger(ILog logger)
            {
                this.logger = logger;
            }

            public void Info(string message)
            {
                logger.Info(message);
            }

            public void Error(string message, Exception ex)
            {
                logger.Error(message, ex);
            }
        }
    }
}