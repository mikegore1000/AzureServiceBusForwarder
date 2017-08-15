using System;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using Orders.Events;

namespace Payments.Handlers
{
    public class OrderAcceptedHandler : IHandleMessages<OrderAccepted>
    {
        private static int MessagesProcessed = 0;

        public Task Handle(OrderAccepted message, IMessageHandlerContext context)
        {
            int val = Interlocked.Increment(ref MessagesProcessed);

            if (val % 100 == 0)
            {
                Console.WriteLine($"Accepted {MessagesProcessed} orders");
            }

            return Task.CompletedTask;
        }
    }
}
