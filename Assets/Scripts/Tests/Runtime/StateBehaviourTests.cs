using NUnit.Framework;
using Moq;

namespace KDMagical.SUSMachine.Tests
{
    public partial class SUSMachineTests
    {
        internal class StateBehaviourTests
        {
            [Test, AutoMoqData]
            public void StateBehaviour_Calls_Actions(IStateMachine<States> stateMachine)
            {
                var actions = new MockStateActions();
                var (enter, exit, update, fixedUpdate, lateUpdate) = actions;

                var sut = new StateBehaviour<States>
                {
                    OnEnter = enter.Object,
                    OnExit = exit.Object,
                    OnUpdate = update.Object,
                    OnFixedUpdate = fixedUpdate.Object,
                    OnLateUpdate = lateUpdate.Object
                };

                sut.Initialize(stateMachine);

                sut.DoEnter();

                enter.Verify(f => f(stateMachine), Times.Once);
                exit.VerifyNoOtherCalls();
                update.VerifyNoOtherCalls();
                fixedUpdate.VerifyNoOtherCalls();
                lateUpdate.VerifyNoOtherCalls();

                sut.DoExit();

                enter.VerifyNoOtherCalls();
                exit.Verify(f => f(stateMachine), Times.Once);
                update.VerifyNoOtherCalls();
                fixedUpdate.VerifyNoOtherCalls();
                lateUpdate.VerifyNoOtherCalls();

                sut.DoUpdate();

                enter.VerifyNoOtherCalls();
                exit.VerifyNoOtherCalls();
                update.Verify(f => f(stateMachine), Times.Once);
                fixedUpdate.VerifyNoOtherCalls();
                lateUpdate.VerifyNoOtherCalls();

                sut.DoFixedUpdate();

                enter.VerifyNoOtherCalls();
                exit.VerifyNoOtherCalls();
                update.VerifyNoOtherCalls();
                fixedUpdate.Verify(f => f(stateMachine), Times.Once);
                lateUpdate.VerifyNoOtherCalls();

                sut.DoLateUpdate();

                enter.VerifyNoOtherCalls();
                exit.VerifyNoOtherCalls();
                update.VerifyNoOtherCalls();
                fixedUpdate.VerifyNoOtherCalls();
                lateUpdate.Verify(f => f(stateMachine), Times.Once);
            }

            [Test, AutoMoqData]
            public void StateBehaviour_Events_Calls_Actions(IStateMachine<States> stateMachine)
            {
                var (enter, exit, update, fixedUpdate, lateUpdate) = new MockStateActions();
                var (event1, event2, event3) = new MockEventActions();

                var sut = new StateBehaviour<States, Events>
                {
                    OnEnter = enter.Object,
                    OnExit = exit.Object,
                    OnUpdate = update.Object,
                    OnFixedUpdate = fixedUpdate.Object,
                    OnLateUpdate = lateUpdate.Object,

                    [Events.Event1] = event1.Object,
                    [Events.Event2] = event2.Object,
                    [Events.Event3] = event3.Object
                };

                sut.Initialize(stateMachine);

                sut.DoEnter();

                enter.Verify(f => f(stateMachine), Times.Once);
                exit.VerifyNoOtherCalls();
                update.VerifyNoOtherCalls();
                fixedUpdate.VerifyNoOtherCalls();
                lateUpdate.VerifyNoOtherCalls();

                sut.DoExit();

                enter.VerifyNoOtherCalls();
                exit.Verify(f => f(stateMachine), Times.Once);
                update.VerifyNoOtherCalls();
                fixedUpdate.VerifyNoOtherCalls();
                lateUpdate.VerifyNoOtherCalls();

                sut.DoUpdate();

                enter.VerifyNoOtherCalls();
                exit.VerifyNoOtherCalls();
                update.Verify(f => f(stateMachine), Times.Once);
                fixedUpdate.VerifyNoOtherCalls();
                lateUpdate.VerifyNoOtherCalls();

                sut.DoFixedUpdate();

                enter.VerifyNoOtherCalls();
                exit.VerifyNoOtherCalls();
                update.VerifyNoOtherCalls();
                fixedUpdate.Verify(f => f(stateMachine), Times.Once);
                lateUpdate.VerifyNoOtherCalls();

                sut.DoLateUpdate();

                enter.VerifyNoOtherCalls();
                exit.VerifyNoOtherCalls();
                update.VerifyNoOtherCalls();
                fixedUpdate.VerifyNoOtherCalls();
                lateUpdate.Verify(f => f(stateMachine), Times.Once);

                sut.TriggerEventAndGetTransition(Events.Event1);

                event1.Verify(f => f(stateMachine), Times.Once);
                event2.VerifyNoOtherCalls();
                event3.VerifyNoOtherCalls();

                sut.TriggerEventAndGetTransition(Events.Event2);

                event1.VerifyNoOtherCalls();
                event2.Verify(f => f(stateMachine), Times.Once);
                event3.VerifyNoOtherCalls();

                sut.TriggerEventAndGetTransition(Events.Event3);

                event1.VerifyNoOtherCalls();
                event2.VerifyNoOtherCalls();
                event3.Verify(f => f(stateMachine), Times.Once);
            }

            [Test]
            [InlineAutoMoqData(Events.Event1, States.State1)]
            [InlineAutoMoqData(Events.Event2, null)]
            public void StateBehaviour_Events_Returns_Transition(
                Events eventToTrigger,
                States? expectedState,
                IStateMachine<States> stateMachine
            )
            {
                var sut = new StateBehaviour<States, Events>
                {
                    Transitions = {
                        {States.State1, Events.Event1}
                    }
                };

                Assert.AreEqual(sut.TriggerEventAndGetTransition(eventToTrigger), expectedState);
            }
        }
    }
}