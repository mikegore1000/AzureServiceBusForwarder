using System;
using System.Threading.Tasks;
using NServiceBus;
using Payments.Events;

namespace Payments.Handlers
{
    public class PaymentAcceptedHandler : IHandleMessages<PaymentAccepted>
    {
        public Task Handle(PaymentAccepted message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Payment accepted {message.PaymentReference}");
            return Task.CompletedTask;
        }
    }
}
