using System;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace AzureServiceBusForwarder
{
    public class ForwarderConfiguration
    {
        public ForwarderConfiguration(ForwarderSourceConfiguration source, ForwarderDestinationConfiguration destination)
        {
            Guard.IsNotNull(source, nameof(source));
            Guard.IsNotNull(destination, nameof(destination));

            this.Source = source;
            this.Destination = destination;
        }

        internal ForwarderSourceConfiguration Source { get; }

        internal ForwarderDestinationConfiguration Destination { get; }

        internal ILogger Logger { get; private set; } = new NullLogger();

        internal int Concurrency { get; private set; } = 1;

        internal Action<Metric> MetricHander { get; private set; } = m => { };

        public ForwarderConfiguration UsingLogger(ILogger logger)
        {
            Guard.IsNotNull(logger, nameof(logger));
            Logger = logger;
            return this;
        }

        public ForwarderConfiguration WithConcurrencyOf(int concurrency)
        {
            Guard.IsGreaterThan(0, concurrency, nameof(concurrency));
            Concurrency = concurrency;
            return this;
        }

        public ForwarderConfiguration UsingMetrics(Action<Metric> metricHandler)
        {
            Guard.IsNotNull(metricHandler, nameof(metricHandler));
            MetricHander = metricHandler;
            return this;
        }
    }
}
