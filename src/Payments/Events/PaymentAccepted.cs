using System;
using NServiceBus;

namespace Payments.Events
{
    public class PaymentAccepted : IEvent
    {
        public Guid PaymentReference { get; set; }
    }
}
