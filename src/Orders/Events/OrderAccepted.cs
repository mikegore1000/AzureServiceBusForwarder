using System;

namespace Orders.Events
{
    public class OrderAccepted
    {
        public Guid OrderReference { get; set; }
    }
}