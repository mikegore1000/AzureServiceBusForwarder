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


        public ForwarderConfiguration UsingLogger(ILogger logger)
        {
            Guard.IsNotNull(logger, nameof(logger));
            Logger = logger;
            return this;
        }
    }
}
