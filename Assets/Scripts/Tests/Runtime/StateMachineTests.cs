using NUnit.Framework;
using Moq;
using UnityEngine;

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
                var anyStateMock = new Mock<Stateless<States>>();
                var stateBehaviourMock = new Mock<Stateless<States>>();

                var sut = new StateMachine<States>(manager)
                {
                    AnyState = anyStateMock.Object,
                    [state] = stateBehaviourMock.Object
                };

                sut.Initialize(state);

                anyStateMock.Verify(s => s.Initialize(sut), Times.Once);
                anyStateMock.Verify(s => s.DoEnter(), Times.Once);

                stateBehaviourMock.Verify(s => s.Initialize(sut), Times.Once);
                stateBehaviourMock.Verify(s => s.DoEnter(), Times.Once);
            }

            [Test, AutoMoqData]
            public void Register_And_Deregister_Called(
                IStateMachineManager manager,
                States state,
                StateAction<States> stateAction)
            {
                var stateManagerMock = Mock.Get<IStateMachineManager>(manager);

                var sut = new StateMachine<States>(manager)
                {
                    [state] = {
                        OnUpdate = stateAction
                    }
                };

                sut.Initialize(state);

                stateManagerMock.Verify(m => m.Register(sut), Times.Once);

                sut.Close();

                stateManagerMock.Verify(m => m.Deregister(sut), Times.Once);
            }

            [Test, AutoMoqData]
            public void Enter_Exit_Actions_Called(IStateMachineManager manager)
            {
                var anyStateMock = new Mock<Stateless<States>>();
                var behaviourMock1 = new Mock<Stateless<States>>();
                var behaviourMock2 = new Mock<Stateless<States>>();

                var sut = new StateMachine<States>(manager)
                {
                    AnyState = anyStateMock.Object,
                    [States.State1] = behaviourMock1.Object,
                    [States.State2] = behaviourMock2.Object
                };

                sut.Initialize(States.State1);
                anyStateMock.Verify(s => s.Initialize(sut), Times.Once);
                behaviourMock1.Verify(s => s.Initialize(sut), Times.Once);
                behaviourMock2.Verify(s => s.Initialize(sut), Times.Once);

                anyStateMock.Verify(s => s.DoEnter(), Times.Once);
                behaviourMock1.Verify(s => s.DoEnter(), Times.Once);

                sut.SetState(States.State2);
                anyStateMock.Verify(s => s.DoEnter(), Times.Exactly(2));
                behaviourMock1.Verify(s => s.DoExit(), Times.Once);
                behaviourMock2.Verify(s => s.DoEnter(), Times.Once);
            }

            [Test, AutoMoqData]
            public void Close_Exit_Actions_Called(IStateMachineManager manager)
            {
                var anyStateMock = new Mock<Stateless<States>>();
                var behaviourMock1 = new Mock<Stateless<States>>();

                var sut = new StateMachine<States>(manager)
                {
                    AnyState = anyStateMock.Object,
                    [States.State1] = behaviourMock1.Object
                };

                sut.Initialize(States.State1);

                sut.Close();
                anyStateMock.Verify(s => s.DoExit(), Times.Once);
                behaviourMock1.Verify(s => s.DoExit(), Times.Once);
            }

            [Test, AutoMoqData]
            public void Tick_Actions_Called(IStateMachineManager manager, States state)
            {
                var anyStateMock = new Mock<Stateless<States>>();
                var behaviourMock = new Mock<Stateless<States>>();

                var sut = new StateMachine<States>(manager)
                {
                    AnyState = anyStateMock.Object,
                    [state] = behaviourMock.Object
                };

                sut.Initialize(state);

                sut.DoUpdate();
                anyStateMock.Verify(
                    x => x.CheckAutoTransitions(TransitionType.Update),
                    Times.Once);
                anyStateMock.Verify(x => x.DoUpdate(), Times.Once);
                behaviourMock.Verify(
                    x => x.CheckAutoTransitions(TransitionType.Update),
                    Times.Once);
                behaviourMock.Verify(x => x.DoUpdate(), Times.Once);

                sut.DoFixedUpdate();
                anyStateMock.Verify(
                    x => x.CheckAutoTransitions(TransitionType.FixedUpdate),
                    Times.Once);
                anyStateMock.Verify(x => x.DoFixedUpdate(), Times.Once);
                behaviourMock.Verify(
                    x => x.CheckAutoTransitions(TransitionType.FixedUpdate),
                    Times.Once);
                behaviourMock.Verify(x => x.DoFixedUpdate(), Times.Once);

                sut.DoLateUpdate();
                anyStateMock.Verify(
                    x => x.CheckAutoTransitions(TransitionType.LateUpdate),
                    Times.Once);
                anyStateMock.Verify(x => x.DoLateUpdate(), Times.Once);
                behaviourMock.Verify(
                    x => x.CheckAutoTransitions(TransitionType.LateUpdate),
                    Times.Once);
                behaviourMock.Verify(x => x.DoLateUpdate(), Times.Once);
            }

            [Test, AutoMoqData]
            public void Event_Triggers_Called(IStateMachineManager manager, States state, Events fsmEvent)
            {
                var anyStateMock = new Mock<Stateless<States, Events>>();
                var behaviourMock = new Mock<Stateless<States, Events>>();

                var sut = new StateMachine<States, Events>
                {
                    AnyState = anyStateMock.Object,
                    [state] = behaviourMock.Object
                };

                sut.Initialize(state);

                sut.TriggerEvent(fsmEvent);

                anyStateMock.Verify(s => s.TriggerEventAndGetTransition(fsmEvent), Times.Once);
                behaviourMock.Verify(s => s.TriggerEventAndGetTransition(fsmEvent), Times.Once);
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
                            {Events.Event1, States.State1}
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
            public void Registration_Stateful_Update_Action_Register_Called(IStateMachineManager manager, States state, StatefulAction<States, int> stateAction)
            {
                var managerMock = Mock.Get(manager);

                var sut = new StateMachine<States>(manager)
                {
                    [state] = new Stateful<States, int>
                    {
                        OnUpdate = stateAction
                    },
                };

                sut.Initialize(state);
                managerMock.Verify(m => m.Register(sut), Times.Once);

                sut.Close();
                managerMock.Verify(m => m.Deregister(sut), Times.Once);
            }

            [Test, AutoMoqData]
            public void Registration_Stateful_Events_Update_Action_Register_Called(IStateMachineManager manager, States state, StatefulAction<States, int> stateAction)
            {
                var managerMock = Mock.Get(manager);

                var sut = new StateMachine<States, Events>(manager)
                {
                    [state] = new Stateful<States, Events, int>
                    {
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

            [Test, AutoMoqData]
            public void Registration_AnyState_Update_Action_Register_Called(IStateMachineManager manager, States state, StateAction<States> stateAction)
            {
                var managerMock = Mock.Get(manager);

                var sut = new StateMachine<States>(manager)
                {
                    AnyState = {
                        OnUpdate = stateAction
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

            [Test, AutoMoqData]
            public void SetState_In_Actions_Causes_StackOverflow(IStateMachineManager manager)
            {
                const string Message = "SetState is being called ad infinitum!";
                int count = 0;

                var sut = new StateMachine<States>(manager)
                {
                    AnyState =
                    {
                        OnEnter = fsm =>
                        {
                            fsm.SetState(States.State2);
                            count++;
                            if (count > 10000)
                            {
                                throw new System.StackOverflowException(Message);
                            }
                        }
                    }
                };

                Assert.Throws<System.StackOverflowException>(() => sut.Initialize(States.State1), Message);
            }

            [Test, AutoMoqData]
            public void Initialize_NoUpdate_No_AutoCloser(IStateMachineManager manager, string gameObjName)
            {
                var managerMock = Mock.Get<IStateMachineManager>(manager);
                var go = new GameObject(gameObjName);

                var sut = new StateMachine<States>(manager);

                sut.Initialize(States.State1, go);

                Assert.IsFalse(go.GetComponent<StateMachineAutoCloser>());

                GameObject.DestroyImmediate(go);
            }

            [Test, AutoMoqData]
            public void Initialize_WithUpdate_Creates_Working_AutoCloser(
                IStateMachineManager manager,
                string gameObjName,
                StateAction<States> stateAction)
            {
                var managerMock = Mock.Get<IStateMachineManager>(manager);
                var go = new GameObject(gameObjName);

                var sut = new StateMachine<States>(manager)
                {
                    AnyState = {
                        OnUpdate = stateAction,
                    }
                };

                sut.Initialize(States.State1, go);

                Assert.IsTrue(go.GetComponent<StateMachineAutoCloser>());

                GameObject.DestroyImmediate(go);

                managerMock.Verify(m => m.Deregister(sut), Times.Once);
            }

            [Test, AutoMoqData]
            public void NextState_AnyState_Set_Correctly(
                IStateMachineManager manager,
                StateAction<States> stateAction,
                States nextState
            )
            {
                var sut = new StateMachine<States>(manager)
                {
                    AnyState =
                    {
                        OnEnter = fsm => Assert.IsNull(fsm.NextState),
                        OnExit = fsm => Assert.AreEqual(nextState, fsm.NextState)
                    }
                };

                sut.Initialize(States.State1);
                sut.SetState(nextState);
            }

            [Test, AutoMoqData]
            public void NextState_Set_Correctly(
                IStateMachineManager manager,
                StateAction<States> stateAction
            )
            {
                var sut = new StateMachine<States>(manager)
                {
                    AnyState =
                    {
                        OnEnter = fsm => Assert.IsNull(fsm.NextState)
                    },

                    [States.State1] =
                    {
                        OnExit = fsm => Assert.AreEqual(States.State2, fsm.NextState)
                    },

                    [States.State2] =
                    {
                        OnExit = fsm => Assert.AreEqual(States.State3, fsm.NextState)
                    },

                    [States.State3] =
                    {
                        OnExit = fsm => Assert.IsNull(fsm.NextState)
                    }
                };

                sut.Initialize(States.State1);
                sut.SetState(States.State2);
                sut.SetState(States.State3);
                sut.Close();
            }

            [Test, AutoMoqData]
            public void PreviousState_Set_Correctly(
                IStateMachineManager manager,
                StateAction<States> stateAction
            )
            {
                bool firstEnter = true;

                var sut = new StateMachine<States>(manager)
                {
                    AnyState =
                    {
                        OnEnter = fsm =>
                        {
                            if (firstEnter)
                            {
                                Assert.IsNull(fsm.PreviousState);
                                firstEnter = false;
                            }
                        }
                    },

                    [States.State1] =
                    {
                        OnEnter = fsm => Assert.IsNull(fsm.PreviousState),
                        OnExit = fsm => Assert.IsNull(fsm.PreviousState),
                    },

                    [States.State2] =
                    {
                        OnEnter = fsm => Assert.AreEqual(States.State1, fsm.PreviousState),
                        OnExit = fsm => Assert.AreEqual(States.State1, fsm.PreviousState),
                    },

                    [States.State3] =
                    {
                        OnEnter = fsm => Assert.AreEqual(States.State2, fsm.PreviousState),
                        OnExit = fsm => Assert.AreEqual(States.State2, fsm.PreviousState),
                    }
                };

                sut.Initialize(States.State1);
                sut.SetState(States.State2);
                sut.SetState(States.State3);
                sut.Close();
            }

            [Test, AutoMoqData]
            public void Events_Called_Before_Specific_State_Transition(
                IStateMachineManager manager,
                Mock<StateAction<States>> shouldBeCalledActionMock,
                Mock<StateAction<States>> shouldNotBeCalledActionMock,
                Events fsmEvent
            )
            {
                var sut = new StateMachine<States, Events>
                {
                    AnyState =
                    {
                        [fsmEvent] = shouldBeCalledActionMock.Object,
                        Transitions = {{fsmEvent, States.State3}}
                    },

                    [States.State1] =
                    {
                        Transitions = {{fsmEvent, States.State2}}
                    },

                    [States.State3] =
                    {
                        OnEnter = shouldNotBeCalledActionMock.Object
                    }
                };

                sut.Initialize(States.State1);
                sut.TriggerEvent(fsmEvent);

                shouldNotBeCalledActionMock.VerifyNoOtherCalls();

                shouldBeCalledActionMock.Verify(a => a(sut), Times.Once);

                Assert.AreEqual(States.State2, sut.CurrentState);
            }
        }
    }
}