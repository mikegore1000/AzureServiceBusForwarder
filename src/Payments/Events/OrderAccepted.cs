using System;
using NServiceBus;

namespace Orders.Events
{
    public class OrderAccepted : IMessage
    {
        public Guid OrderReference { get; set; }
    }
}
