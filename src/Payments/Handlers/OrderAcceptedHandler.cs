using System;
using System.Threading.Tasks;
using NServiceBus;
using Orders.Events;

namespace Payments.Handlers
{
    public class OrderAcceptedHandler : IHandleMessages<OrderAccepted>
    {
        public Task Handle(OrderAccepted message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Accepted order {message.OrderReference}");

            return Task.FromResult(0);
        }
    }
}
