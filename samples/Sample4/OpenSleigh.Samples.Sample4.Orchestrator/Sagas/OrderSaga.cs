using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenSleigh.Core;
using OpenSleigh.Core.Messaging;
using OpenSleigh.Samples.Sample4.Common;

namespace OpenSleigh.Samples.Sample4.Orchestrator.Sagas
{
    public record OrderSagaState : SagaState{
        public OrderSagaState(Guid id) : base(id){}

        public Guid OrderId { get; set; }
        public bool CreditCheckCompleted { get; set; } = false;
        public bool InventoryCheckCompleted{ get; set; } = false;
    }

    public class OrderSaga :
        Saga<OrderSagaState>,
        IStartedBy<SaveOrder>,
        IHandleMessage<CrediCheckCompleted>,
        IHandleMessage<InventoryCheckCompleted>,
        IHandleMessage<ShippingCompleted>
    {
        private readonly ILogger<OrderSaga> _logger;

        public OrderSaga(ILogger<OrderSaga> logger, OrderSagaState state) : base(state)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public async Task HandleAsync(IMessageContext<SaveOrder> context, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"processing order '{context.Message.OrderId}'...");

            this.State.OrderId = context.Message.OrderId;

            var startCreditCheck = ProcessCreditCheck.New(context.Message.OrderId);
            this.Publish(startCreditCheck);

            var startInventoryCheck = CheckInventory.New(context.Message.OrderId);
            this.Publish(startInventoryCheck);
        }
        
        public async Task HandleAsync(IMessageContext<CrediCheckCompleted> context, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"credit check for order '{context.Message.OrderId}' completed!");

            this.State.CreditCheckCompleted = true;

            if (CheckCanShipOrder(cancellationToken))
            {
                var message = ProcessShipping.New(this.State.OrderId);
                this.Publish(message);
            }
        }

        public async Task HandleAsync(IMessageContext<InventoryCheckCompleted> context, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"inventory check for order '{context.Message.OrderId}' completed!");

            this.State.InventoryCheckCompleted = true;

            if (CheckCanShipOrder(cancellationToken))
            {
                var message = ProcessShipping.New(this.State.OrderId);
                this.Publish(message);
            }
        }

        public async Task HandleAsync(IMessageContext<ShippingCompleted> context, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Processing completed for order '{context.Message.OrderId}'");

            var message = OrderSagaCompleted.New(this.State.OrderId);
            this.Publish(message);

            this.State.MarkAsCompleted();
        }
        
        private bool CheckCanShipOrder(CancellationToken cancellationToken = default)
        {
            var checksFulfilled = this.State.CreditCheckCompleted &&
                                  this.State.InventoryCheckCompleted;
            return checksFulfilled;
        }

    }
}
