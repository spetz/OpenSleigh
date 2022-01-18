﻿using System;
using FluentAssertions;
using OpenSleigh.Core.Tests.Sagas;
using Xunit;

namespace OpenSleigh.Core.Tests.Unit
{
    public class SagaTests
    {
        [Fact]
        public void ctor_should_throw_when_state_null()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new DummySaga(null));
            ex.ParamName.Should().Be("state");
        }

        [Fact]
        public void SetState_should_set_state()
        {
            var state = new DummySagaState(Guid.NewGuid());
            var sut = new DummySaga(state);
            sut.State.Should().Be(state);
        }

        [Fact]
        public void Publish_should_add_message_to_outbox()
        {
            var message = DummyMessage.New();

            var state = new DummySagaState(Guid.NewGuid());
            var sut = new DummySaga(state);
            sut.PublishTestWrapper(message);

            state.Outbox.Should().Contain(message);
        }
    }
}
