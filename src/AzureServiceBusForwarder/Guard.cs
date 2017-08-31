using System;

namespace AzureServiceBusForwarder
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

        public static void IsGreaterThan(int expected, int value, string name)
        {
            if (expected >= value)
            {
                throw new ArgumentOutOfRangeException($"{name} must be greater than {expected}");
            }
        }
    }
}