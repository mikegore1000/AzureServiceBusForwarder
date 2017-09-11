using System;

namespace AzureServiceBusForwarder
{
    public interface ILogger
    {
        void Debug(string message);

        void Error(string message, Exception ex);
    }
}