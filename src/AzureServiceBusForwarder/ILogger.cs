using System;

namespace AzureServiceBusForwarder
{
    public interface ILogger
    {
        void Info(string message);

        void Error(string message, Exception ex);
    }
}