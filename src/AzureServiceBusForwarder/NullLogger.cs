namespace AzureServiceBusForwarder
{
    public class NullLogger : ILogger
    {
        public void Info(string message)
        {
        }
    }
}