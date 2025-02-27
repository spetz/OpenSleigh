using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenSleigh.Core;
using OpenSleigh.Core.Messaging;
using OpenSleigh.Samples.Sample10.Common;

namespace OpenSleigh.Samples.Sample10.PaymentService.Sagas
{
    public record CreditCheckSagaState : SagaState{
        public CreditCheckSagaState(Guid id) : base(id){}
    }
    
    public class CreditCheckSaga :
        Saga<CreditCheckSagaState>,
        IStartedBy<ProcessCreditCheck>
    {
        private readonly ILogger<CreditCheckSaga> _logger;

        public CreditCheckSaga(ILogger<CreditCheckSaga> logger, CreditCheckSagaState state) : base(state)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public async Task HandleAsync(IMessageContext<ProcessCreditCheck> context, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"processing credit check for order '{context.Message.OrderId}'...");
            
            var message = CrediCheckCompleted.New(context.Message.OrderId);
            this.Publish(message);

            _logger.LogInformation($"credit check for order '{context.Message.OrderId}' processed!");

            this.State.MarkAsCompleted();
        }
    }
}
