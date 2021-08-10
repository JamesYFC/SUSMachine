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

        public struct StateData
        {
            public int SomeNum;
            public string SomeString;

            public StateData(int someNum, string someString)
            {
                this.SomeNum = someNum;
                this.SomeString = someString;
            }
        }

        public class MockStateActions
        {

            public Mock<StateAction<States>> Enter { get; } = new Mock<StateAction<States>>();
            public Mock<StateAction<States>> Exit { get; } = new Mock<StateAction<States>>();
            public Mock<StateAction<States>> Update { get; } = new Mock<StateAction<States>>();
            public Mock<StateAction<States>> FixedUpdate { get; } = new Mock<StateAction<States>>();
            public Mock<StateAction<States>> LateUpdate { get; } = new Mock<StateAction<States>>();

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

        public class MockStatefulActions<T> where T : struct
        {

            public Mock<StatefulAction<States, T>> Enter { get; } = new Mock<StatefulAction<States, T>>();
            public Mock<StatefulAction<States, T>> Exit { get; } = new Mock<StatefulAction<States, T>>();
            public Mock<StatefulAction<States, T>> Update { get; } = new Mock<StatefulAction<States, T>>();
            public Mock<StatefulAction<States, T>> FixedUpdate { get; } = new Mock<StatefulAction<States, T>>();
            public Mock<StatefulAction<States, T>> LateUpdate { get; } = new Mock<StatefulAction<States, T>>();

            public void Deconstruct(
                out Mock<StatefulAction<States, T>> enter,
                out Mock<StatefulAction<States, T>> exit,
                out Mock<StatefulAction<States, T>> update,
                out Mock<StatefulAction<States, T>> fixedUpdate,
                out Mock<StatefulAction<States, T>> lateUpdate
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

            public Mock<StateAction<States>> Event1Action { get; } = new Mock<StateAction<States>>();
            public Mock<StateAction<States>> Event2Action { get; } = new Mock<StateAction<States>>();
            public Mock<StateAction<States>> Event3Action { get; } = new Mock<StateAction<States>>();

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

        public class MockStatefulEventActions<T> where T : struct
        {

            public Mock<StatefulAction<States, T>> Event1Action { get; } = new Mock<StatefulAction<States, T>>();
            public Mock<StatefulAction<States, T>> Event2Action { get; } = new Mock<StatefulAction<States, T>>();
            public Mock<StatefulAction<States, T>> Event3Action { get; } = new Mock<StatefulAction<States, T>>();

            public void Deconstruct(
                out Mock<StatefulAction<States, T>> event1Action,
                out Mock<StatefulAction<States, T>> event2Action,
                out Mock<StatefulAction<States, T>> event3Action
            )
            {
                event1Action = Event1Action;
                event2Action = Event2Action;
                event3Action = Event3Action;
            }
        }
    }
}