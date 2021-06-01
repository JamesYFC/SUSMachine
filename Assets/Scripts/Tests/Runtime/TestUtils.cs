using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;
using Moq;

namespace KDMagical.SUSMachine.Tests
{
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute()
            : base(() => new Fixture()
                .Customize(new AutoMoqCustomization()))
        {
        }
    }

    public class InlineAutoMoqDataAttribute : InlineAutoDataAttribute
    {
        public InlineAutoMoqDataAttribute(params object[] arguments)
            : base(
                () => new Fixture()
                    .Customize(new AutoMoqCustomization()),
                arguments)
        {
        }
    }

    public partial class SUSMachineTests
    {
        public enum States { State1, State2, State3 }
        public enum Events { Event1, Event2, Event3 }

        public class MockStateActions
        {

            public Mock<StateAction<States>> Enter { get; } = CreateActionMock();
            public Mock<StateAction<States>> Exit { get; } = CreateActionMock();
            public Mock<StateAction<States>> Update { get; } = CreateActionMock();
            public Mock<StateAction<States>> FixedUpdate { get; } = CreateActionMock();
            public Mock<StateAction<States>> LateUpdate { get; } = CreateActionMock();

            public void Deconstruct(
                out Mock<StateAction<States>> enter,
                out Mock<StateAction<States>> exit,
                out Mock<StateAction<States>> update,
                out Mock<StateAction<States>> fixedUpdate,
                out Mock<StateAction<States>> lateUpdate
            )
            {
                enter = Enter;
                exit = Exit;
                update = Update;
                fixedUpdate = FixedUpdate;
                lateUpdate = LateUpdate;
            }
        }

        public class MockEventActions
        {

            public Mock<StateAction<States>> Event1Action { get; } = CreateActionMock();
            public Mock<StateAction<States>> Event2Action { get; } = CreateActionMock();
            public Mock<StateAction<States>> Event3Action { get; } = CreateActionMock();

            public void Deconstruct(
                out Mock<StateAction<States>> event1Action,
                out Mock<StateAction<States>> event2Action,
                out Mock<StateAction<States>> event3Action
            )
            {
                event1Action = Event1Action;
                event2Action = Event2Action;
                event3Action = Event3Action;
            }
        }

        private static Mock<StateAction<States>> CreateActionMock()
        {
            return new Mock<StateAction<States>>();
        }
    }
}