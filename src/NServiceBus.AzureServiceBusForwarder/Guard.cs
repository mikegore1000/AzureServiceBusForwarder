using System;

namespace NServiceBus.AzureServiceBusForwarder
{
    internal static class Guard
    {
        internal static void IsNotNullOrEmpty(string value, string name)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException($"A {name} value must be supplied", name);
            }
        }

        public static void IsNotNull(object value, string name)
        {
            if (value == null)
            {
                throw new ArgumentNullException($"A {name} value must be supplied", name);
            }
        }
    }
}