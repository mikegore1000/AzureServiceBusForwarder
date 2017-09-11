using System;

namespace AzureServiceBusForwarder
{
    public class NullLogger : ILogger
    {
        public void Debug(string message)
        {
        }

        public void Error(string message, Exception ex)
        {
        }
    }
}