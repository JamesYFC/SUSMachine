using NUnit.Framework;
using Moq;

namespace KDMagical.SUSMachine.Tests
{
    public partial class SUSMachineTests
    {
        internal class StateMachineTests<T>
        {
            [Test, AutoMoqData]
            public void Initialized_And_Enter_Called(
                IStateMachineManager manager,
                States state)
            {
                var stateBehaviourMock = new Mock<StateBehaviour<States>>();

                var sut = new StateMachine<States>(manager)
                {
                    [state] = stateBehaviourMock.Object
                };

                sut.Initialize(state);

                stateBehaviourMock.Verify(s => s.Initialize(sut), Times.Once);
                stateBehaviourMock.Verify(s => s.DoEnter(), Times.Once);
                stateBehaviourMock.VerifyNoOtherCalls();
            }

            [Test, AutoMoqData]
            public void Register_And_Deregister_Called(
                IStateMachineManager manager,
                States state)
            {
                var stateBehaviourMock = new Mock<StateBehaviour<States>>();

                var sut = new StateMachine<States>(manager)
                {
                    [state] = stateBehaviourMock.Object
                };

                var stateManagerMock = Mock.Get<IStateMachineManager>(manager);

                sut.Initialize(state);

                stateManagerMock.Verify(m => m.Register(sut), Times.Once);

                sut.Close();

                stateManagerMock.Verify(m => m.Deregister(sut), Times.Once);
            }

            [Test, AutoMoqData]
            public void Enter_Exit_Actions_Called(IStateMachineManager manager)
            {
                var behaviourMock1 = new Mock<StateBehaviour<States>>();
                var behaviourMock2 = new Mock<StateBehaviour<States>>();

                var sut = new StateMachine<States>(manager)
                {
                    [States.State1] = behaviourMock1.Object,
                    [States.State2] = behaviourMock2.Object
                };

                sut.Initialize(States.State1);
                behaviourMock1.Verify(s => s.Initialize(sut), Times.Once);
                behaviourMock2.Verify(s => s.Initialize(sut), Times.Once);
                behaviourMock1.Verify(s => s.DoEnter(), Times.Once);
                behaviourMock1.VerifyNoOtherCalls();
                behaviourMock2.VerifyNoOtherCalls();

                sut.SetState(States.State2);
                behaviourMock1.Verify(s => s.DoExit(), Times.Once);
                behaviourMock2.Verify(s => s.DoEnter(), Times.Once);
                behaviourMock1.VerifyNoOtherCalls();
                behaviourMock2.VerifyNoOtherCalls();
            }

            [Test, AutoMoqData]
            public void Tick_Actions_Called(IStateMachineManager manager, States state)
            {
                var behaviourMock = new Mock<StateBehaviour<States>>();

                var sut = new StateMachine<States>(manager)
                {
                    [state] = behaviourMock.Object,
                };

                sut.Initialize(state);
                behaviourMock.Verify(s => s.Initialize(sut), Times.Once);
                behaviourMock.Verify(s => s.DoEnter(), Times.Once);
                behaviourMock.VerifyNoOtherCalls();

                sut.DoUpdate();
                behaviourMock.Verify(
                    x => x.CheckAutoTransitions(TransitionType.Update),
                    Times.Once);
                behaviourMock.Verify(x => x.DoUpdate(), Times.Once);
                behaviourMock.VerifyNoOtherCalls();

                sut.DoFixedUpdate();
                behaviourMock.Verify(
                    x => x.CheckAutoTransitions(TransitionType.FixedUpdate),
                    Times.Once);
                behaviourMock.Verify(x => x.DoFixedUpdate(), Times.Once);
                behaviourMock.VerifyNoOtherCalls();

                sut.DoLateUpdate();
                behaviourMock.Verify(
                    x => x.CheckAutoTransitions(TransitionType.LateUpdate),
                    Times.Once);
                behaviourMock.Verify(x => x.DoLateUpdate(), Times.Once);
                behaviourMock.VerifyNoOtherCalls();
            }
        }
    }
}