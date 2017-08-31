using System;
using NServiceBus;

namespace Payments.Events
{
    public class OrderAccepted : IMessage
    {
        public Guid OrderReference { get; set; }
    }
}
