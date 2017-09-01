using System;

namespace AzureServiceBusForwarder
{
    public class NullLogger : ILogger
    {
        public void Info(string message)
        {
        }

        public void Error(string message, Exception ex)
        {
        }
    }
}