using NUnit.Framework;
using Moq;

namespace KDMagical.SUSMachine.Tests
{
    public partial class SUSMachineTests
    {
        internal class StateMachineTests
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
            }

            [Test, AutoMoqData]
            public void Register_And_Deregister_Called(
                IStateMachineManager manager,
                States state,
                StateAction<States> stateAction)
            {
                var sut = new StateMachine<States>(manager)
                {
                    [state] = {
                        OnUpdate = stateAction
                    }
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

                sut.SetState(States.State2);
                behaviourMock1.Verify(s => s.DoExit(), Times.Once);
                behaviourMock2.Verify(s => s.DoEnter(), Times.Once);
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

                sut.DoUpdate();
                behaviourMock.Verify(
                    x => x.CheckAutoTransitions(TransitionType.Update),
                    Times.Once);
                behaviourMock.Verify(x => x.DoUpdate(), Times.Once);

                sut.DoFixedUpdate();
                behaviourMock.Verify(
                    x => x.CheckAutoTransitions(TransitionType.FixedUpdate),
                    Times.Once);
                behaviourMock.Verify(x => x.DoFixedUpdate(), Times.Once);

                sut.DoLateUpdate();
                behaviourMock.Verify(
                    x => x.CheckAutoTransitions(TransitionType.LateUpdate),
                    Times.Once);
                behaviourMock.Verify(x => x.DoLateUpdate(), Times.Once);
            }

            [Test, AutoMoqData]
            public void Registration_No_Behaviours_Register_Not_Called(IStateMachineManager manager, States state)
            {
                var managerMock = Mock.Get(manager);

                var sut = new StateMachine<States>(manager);

                sut.Initialize(state);
                managerMock.Verify(m => m.Register(sut), Times.Never);

                sut.Close();
                managerMock.Verify(m => m.Deregister(sut), Times.Never);
            }

            [Test, AutoMoqData]
            public void Registration_No_Update_Methods_Register_Not_Called(IStateMachineManager manager, States state, StateAction<States> stateAction)
            {
                var managerMock = Mock.Get(manager);

                var sut = new StateMachine<States, Events>(manager)
                {
                    [States.State1] = {
                        OnEnter = stateAction
                    },

                    [States.State2] = {
                        OnExit = stateAction,

                        Transitions = {
                            {States.State1, Events.Event1}
                        }
                    }
                };

                sut.Initialize(state);
                managerMock.Verify(m => m.Register(sut), Times.Never);

                sut.Close();
                managerMock.Verify(m => m.Deregister(sut), Times.Never);
            }

            [Test, AutoMoqData]
            public void Registration_Update_Action_Register_Called(IStateMachineManager manager, States state, StateAction<States> stateAction)
            {
                var managerMock = Mock.Get(manager);

                var sut = new StateMachine<States>(manager)
                {
                    [state] = {
                        OnUpdate = stateAction
                    },
                };

                sut.Initialize(state);
                managerMock.Verify(m => m.Register(sut), Times.Once);

                sut.Close();
                managerMock.Verify(m => m.Deregister(sut), Times.Once);
            }

            [Test, AutoMoqData]
            public void Registration_FixedUpdate_Action_Register_Called(IStateMachineManager manager, States state, StateAction<States> stateAction)
            {
                var managerMock = Mock.Get(manager);

                var sut = new StateMachine<States>(manager)
                {
                    [state] = {
                        OnFixedUpdate = stateAction
                    },
                };

                sut.Initialize(state);
                managerMock.Verify(m => m.Register(sut), Times.Once);

                sut.Close();
                managerMock.Verify(m => m.Deregister(sut), Times.Once);
            }

            [Test, AutoMoqData]
            public void Registration_LateUpdate_Action_Register_Called(IStateMachineManager manager, States state, StateAction<States> stateAction)
            {
                var managerMock = Mock.Get(manager);

                var sut = new StateMachine<States>(manager)
                {
                    [state] = {
                        OnLateUpdate = stateAction
                    },
                };

                sut.Initialize(state);
                managerMock.Verify(m => m.Register(sut), Times.Once);

                sut.Close();
                managerMock.Verify(m => m.Deregister(sut), Times.Once);
            }

            [Test]
            [InlineAutoMoqData(TransitionType.Update)]
            [InlineAutoMoqData(TransitionType.FixedUpdate)]
            [InlineAutoMoqData(TransitionType.LateUpdate)]
            public void Registration_Update_Action_Register_Called(
                TransitionType transitionType,
                IStateMachineManager manager,
                States state)
            {
                var managerMock = Mock.Get(manager);

                var sut = new StateMachine<States>(manager)
                {
                    [state] = {
                        Transitions = {
                            {_ => true, state, transitionType}
                        }
                    },
                };

                sut.Initialize(state);
                managerMock.Verify(m => m.Register(sut), Times.Once);

                sut.Close();
                managerMock.Verify(m => m.Deregister(sut), Times.Once);
            }
        }
    }
}