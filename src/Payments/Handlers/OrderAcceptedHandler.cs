using System;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using Orders.Events;

namespace Payments.Handlers
{
    public class OrderAcceptedHandler : IHandleMessages<OrderAccepted>
    {
        private static int ordersAccepted;

        public Task Handle(OrderAccepted message, IMessageHandlerContext context)
        {
            var acceptedCount = Interlocked.Increment(ref ordersAccepted);

            if (acceptedCount % 100 == 0)
            {
                Console.WriteLine($"Accepted {acceptedCount} orders");
            }

            return Task.CompletedTask;
        }
    }
}
