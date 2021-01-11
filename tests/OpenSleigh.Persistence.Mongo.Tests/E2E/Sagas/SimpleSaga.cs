﻿using System;
using System.Threading;
using System.Threading.Tasks;
using OpenSleigh.Core;
using OpenSleigh.Core.Messaging;

namespace OpenSleigh.Persistence.Mongo.Tests.E2E.Sagas
{
    public class SimpleSagaState : SagaState
    {
        public SimpleSagaState(Guid id) : base(id)
        {
        }
    }

    public record StartSimpleSaga(Guid Id, Guid CorrelationId) : ICommand;

    public class SimpleSaga : Saga<SimpleSagaState>, IStartedBy<StartSimpleSaga>
    {
        private readonly Action<StartSimpleSaga> _onStart;

        public SimpleSaga(Action<StartSimpleSaga> onStart)
        {
            _onStart = onStart;
        }

        public Task HandleAsync(IMessageContext<StartSimpleSaga> context, CancellationToken cancellationToken = default)
        {
            _onStart?.Invoke(context.Message);
            return Task.CompletedTask;
        }
    }
}