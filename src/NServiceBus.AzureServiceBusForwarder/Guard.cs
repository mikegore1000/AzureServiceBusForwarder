using System;

namespace NServiceBus.AzureServiceBusForwarder
{
    internal static class Guard
    {
        internal static void IsNullOrEmpty(string value, string name)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException($"A {name} value must be supplied", name);
            }
        }
    }
}