using Moq;
using NUnit.Framework;

namespace KDMagical.SUSMachine.Tests
{
    public partial class SUSMachineTests
    {
        internal class AutoTransitionTests
        {
            internal class StatelessTests
            {
                [Test, AutoMoqData]
                public void TransitionCheck_ShouldCall_AutoTransition_With_Params(
                    IStateMachine<States> stateMachine,
                    Mock<Transition<States>> updateTransitionMock,
                        Mock<Transition<States>> fixedUpdateTransitionMock,
                        Mock<Transition<States>> lateUpdateTransitionMock)

                {
                    var sut = new StatelessTransitions<States>
                {
                    {updateTransitionMock.Object, TransitionType.Update},
                    {fixedUpdateTransitionMock.Object, TransitionType.FixedUpdate},
                    {lateUpdateTransitionMock.Object, TransitionType.LateUpdate}
                };

                    sut.Initialize(stateMachine);

                    updateTransitionMock.VerifyNoOtherCalls();
                    fixedUpdateTransitionMock.VerifyNoOtherCalls();
                    lateUpdateTransitionMock.VerifyNoOtherCalls();

                    sut.CheckTransitions(TransitionType.Update);

                    updateTransitionMock.Verify(f => f(stateMachine), Times.Once);
                    fixedUpdateTransitionMock.VerifyNoOtherCalls();
                    lateUpdateTransitionMock.VerifyNoOtherCalls();

                    sut.CheckTransitions(TransitionType.FixedUpdate);

                    updateTransitionMock.Verify(f => f(stateMachine), Times.Once);
                    fixedUpdateTransitionMock.Verify(f => f(stateMachine), Times.Once);
                    lateUpdateTransitionMock.VerifyNoOtherCalls();

                    sut.CheckTransitions(TransitionType.LateUpdate);

                    updateTransitionMock.Verify(f => f(stateMachine), Times.Once);
                    fixedUpdateTransitionMock.Verify(f => f(stateMachine), Times.Once);
                    lateUpdateTransitionMock.Verify(f => f(stateMachine), Times.Once);
                }

                [Test, AutoMoqData]
                public void TransitionCheck_Returns_NextState(IStateMachine<States> stateMachine)
                {
                    var sut = new StatelessTransitions<States>
                {
                    {_ => States.State1, TransitionType.Update},
                    {_ => null, TransitionType.FixedUpdate}
                };

                    sut.Initialize(stateMachine);

                    Assert.AreEqual(sut.CheckTransitions(TransitionType.Update), States.State1);
                    Assert.IsNull(sut.CheckTransitions(TransitionType.FixedUpdate));
                    Assert.IsNull(sut.CheckTransitions(TransitionType.LateUpdate));
                }

                [Test, AutoMoqData]
                public void TransitionCheck_Returns_Null_On_NoTransitions(IStateMachine<States> stateMachine)
                {
                    var sut = new StatelessTransitions<States> { };

                    sut.Initialize(stateMachine);

                    Assert.IsNull(sut.CheckTransitions(TransitionType.Update));
                    Assert.IsNull(sut.CheckTransitions(TransitionType.FixedUpdate));
                    Assert.IsNull(sut.CheckTransitions(TransitionType.LateUpdate));
                }

                [Test, AutoMoqData]
                public void TransitionCheck_ShouldRespect_TransitionOrder(
                    IStateMachine<States> stateMachine,
                    TransitionType transitionType)
                {
                    var sut = new StatelessTransitions<States>
                {
                    {_ => States.State1, transitionType},
                    {_ => States.State2, transitionType},
                };

                    sut.Initialize(stateMachine);

                    Assert.AreEqual(sut.CheckTransitions(transitionType), States.State1);
                }

                [Test]
                [InlineAutoMoqData(-1, States.State1)]
                [InlineAutoMoqData(2, States.State2)]
                [InlineAutoMoqData(3, null)]
                public void TransitionCheck_SimpleForm_Returns_CorrectState(
                    int num,
                    States? expectedResult,
                    TransitionType transitionType,
                    IStateMachine<States> stateMachine)
                {
                    var sut = new StatelessTransitions<States>
                {
                    {_ => num < 0, States.State1, transitionType},
                    {_ => num % 2 == 0, States.State2, transitionType},
                };

                    sut.Initialize(stateMachine);

                    Assert.AreEqual(sut.CheckTransitions(transitionType), expectedResult);
                }

                [Test]
                [InlineAutoMoqData(-1, States.State1)]
                [InlineAutoMoqData(2, States.State2)]
                [InlineAutoMoqData(3, States.State3)]
                [InlineAutoMoqData(400, null)]
                public void TransitionCheck_Mixed_Returns_CorrectState(
                    int num,
                    States? expectedResult,
                    TransitionType transitionType,
                    IStateMachine<States> stateMachine)
                {
                    var sut = new StatelessTransitions<States>
                {
                    {_ => num < 0 ? (States?)States.State1 : null, transitionType},
                    {_ => {
                        if (num > 100)
                        {
                            return null;
                        }
                        return num % 2 == 0 ? States.State2 : States.State3;
                    }, transitionType},
                    {
                        _ => {
                            var someNum = 50;
                            switch (someNum)
                            {
                                case 1:
                                    return States.State1;
                                case 2:
                                    return States.State2;
                                case 3:
                                    return States.State3;
                                default:
                                    return null;
                            }
                        }
                    }
                };

                    sut.Initialize(stateMachine);

                    Assert.AreEqual(sut.CheckTransitions(transitionType), expectedResult);
                }

                [Test]
                [InlineAutoMoqData(Events.Event1, States.State1)]
                [InlineAutoMoqData(Events.Event2, null)]
                public void TransitionCheck_Events_SuperSimpleForm_Returns_CorrectState(
                    Events fsmEventToTrigger,
                    States? expectedState,
                    IStateMachine<States> stateMachine)
                {
                    var sut = new StatelessTransitions<States, Events> { { States.State1, Events.Event1 } };

                    sut.Initialize(stateMachine);

                    Assert.AreEqual(sut.CheckEventTransitions(fsmEventToTrigger), expectedState);
                }

                [Test]
                [InlineAutoMoqData(Events.Event1, true, States.State1)]
                [InlineAutoMoqData(Events.Event1, false, null)]
                [InlineAutoMoqData(Events.Event2, true, null)]
                [InlineAutoMoqData(Events.Event2, false, null)]
                public void TransitionCheck_Events_SimpleForm_Returns_CorrectState(
                    Events fsmEventToTrigger,
                    bool conditionShouldPass,
                    States? expectedState,
                    IStateMachine<States> stateMachine)
                {
                    var sut = new StatelessTransitions<States, Events> { {
                    _ => conditionShouldPass,
                    States.State1,
                    Events.Event1
                } };

                    sut.Initialize(stateMachine);

                    Assert.AreEqual(sut.CheckEventTransitions(fsmEventToTrigger), expectedState);
                }

                [Test]
                [InlineAutoMoqData(Events.Event1, 1, States.State1)]
                [InlineAutoMoqData(Events.Event1, 2, States.State2)]
                [InlineAutoMoqData(Events.Event1, 100, States.State3)]
                [InlineAutoMoqData(Events.Event1, 0, null)]
                [InlineAutoMoqData(Events.Event2, 1, null)]
                [InlineAutoMoqData(Events.Event2, 0, null)]
                public void TransitionCheck_Events_ComplexForm_Returns_CorrectState(
                    Events fsmEventToTrigger,
                    int num,
                    States? expectedState,
                    IStateMachine<States> stateMachine)
                {
                    var sut = new StatelessTransitions<States, Events> { {
                    _ => num switch
                    {
                        1 => States.State1,
                        2 => States.State2,
                        var x when x >= 3 => States.State3,
                        _ => null
                    },
                    Events.Event1
                } };

                    sut.Initialize(stateMachine);

                    Assert.AreEqual(sut.CheckEventTransitions(fsmEventToTrigger), expectedState);
                }

                [Test]
                [InlineAutoMoqData(Events.Event1, 1, States.State1)]
                [InlineAutoMoqData(Events.Event1, -1, States.State1)]
                [InlineAutoMoqData(Events.Event2, 1, States.State3)]
                [InlineAutoMoqData(Events.Event2, -4, States.State2)]
                [InlineAutoMoqData(Events.Event2, -5, null)]
                public void TransitionCheck_Events_Mixed_Returns_CorrectState(
                    Events fsmEventToTrigger,
                    int num,
                    States? expectedState,
                    IStateMachine<States> stateMachine)
                {
                    var sut = new StatelessTransitions<States, Events> {
                    {States.State1, Events.Event1},
                    {_ => num > 0, States.State2, Events.Event1}, // event1 should never return State2
                    {_ => num > 0, States.State3, Events.Event2},
                    {_ => num % 2 == 0 ? States.State2 : (States?)null, Events.Event2}
                };

                    sut.Initialize(stateMachine);

                    Assert.AreEqual(sut.CheckEventTransitions(fsmEventToTrigger), expectedState);
                }
            }

            internal class StatefulTests
            {
                [Test, AutoMoqData]
                public void TransitionCheck_ShouldCall_AutoTransition_With_Params(
                    IStateMachine<States> stateMachine,
                    StateData someData,
                    Mock<StatefulTransition<States, StateData>> updateTransitionMock,
                    Mock<StatefulTransition<States, StateData>> fixedUpdateTransitionMock,
                    Mock<StatefulTransition<States, StateData>> lateUpdateTransitionMock)
                {
                    var sut = new StatefulTransitions<States, StateData>
                    {
                        {updateTransitionMock.Object, TransitionType.Update},
                        {fixedUpdateTransitionMock.Object, TransitionType.FixedUpdate},
                        {lateUpdateTransitionMock.Object, TransitionType.LateUpdate}
                    };

                    var statefulContainer = new StatefulContainer<StateData>(someData);
                    sut.SetDataContainer(statefulContainer);
                    sut.Initialize(stateMachine);

                    statefulContainer.ResetData();

                    updateTransitionMock.VerifyNoOtherCalls();
                    fixedUpdateTransitionMock.VerifyNoOtherCalls();
                    lateUpdateTransitionMock.VerifyNoOtherCalls();

                    sut.CheckTransitions(TransitionType.Update);

                    updateTransitionMock.Verify(f => f(stateMachine, someData), Times.Once);
                    fixedUpdateTransitionMock.VerifyNoOtherCalls();
                    lateUpdateTransitionMock.VerifyNoOtherCalls();

                    sut.CheckTransitions(TransitionType.FixedUpdate);

                    updateTransitionMock.VerifyNoOtherCalls();
                    fixedUpdateTransitionMock.Verify(f => f(stateMachine, someData), Times.Once);
                    lateUpdateTransitionMock.VerifyNoOtherCalls();

                    sut.CheckTransitions(TransitionType.LateUpdate);

                    updateTransitionMock.VerifyNoOtherCalls();
                    fixedUpdateTransitionMock.VerifyNoOtherCalls();
                    lateUpdateTransitionMock.Verify(f => f(stateMachine, someData), Times.Once);
                }

                internal class DatalessAPI
                {

                    [Test, AutoMoqData]
                    public void TransitionCheck_ShouldCall_BasicTransition_With_Params(
                        IStateMachine<States> stateMachine,
                        StateData someData,
                        Mock<Transition<States>> updateTransitionMock,
                        Mock<Transition<States>> fixedUpdateTransitionMock,
                        Mock<Transition<States>> lateUpdateTransitionMock)
                    {
                        var sut = new StatefulTransitions<States, StateData>
                    {
                        {updateTransitionMock.Object, TransitionType.Update},
                        {fixedUpdateTransitionMock.Object, TransitionType.FixedUpdate},
                        {lateUpdateTransitionMock.Object, TransitionType.LateUpdate}
                    };

                        sut.SetDataContainer(new StatefulContainer<StateData>(someData));
                        sut.Initialize(stateMachine);

                        updateTransitionMock.VerifyNoOtherCalls();
                        fixedUpdateTransitionMock.VerifyNoOtherCalls();
                        lateUpdateTransitionMock.VerifyNoOtherCalls();

                        sut.CheckTransitions(TransitionType.Update);

                        updateTransitionMock.Verify(f => f(stateMachine), Times.Once);
                        fixedUpdateTransitionMock.VerifyNoOtherCalls();
                        lateUpdateTransitionMock.VerifyNoOtherCalls();

                        sut.CheckTransitions(TransitionType.FixedUpdate);

                        updateTransitionMock.VerifyNoOtherCalls();
                        fixedUpdateTransitionMock.Verify(f => f(stateMachine), Times.Once);
                        lateUpdateTransitionMock.VerifyNoOtherCalls();

                        sut.CheckTransitions(TransitionType.LateUpdate);

                        updateTransitionMock.VerifyNoOtherCalls();
                        fixedUpdateTransitionMock.VerifyNoOtherCalls();
                        lateUpdateTransitionMock.Verify(f => f(stateMachine), Times.Once);
                    }

                    [Test, AutoMoqData]
                    public void TransitionCheck_Returns_NextState(IStateMachine<States> stateMachine, StateData someData)
                    {
                        var sut = new StatefulTransitions<States, StateData>
                    {
                        {_ => States.State1, TransitionType.Update},
                        {_ => null, TransitionType.FixedUpdate}
                    };

                        sut.SetDataContainer(new StatefulContainer<StateData>(someData));
                        sut.Initialize(stateMachine);

                        Assert.AreEqual(sut.CheckTransitions(TransitionType.Update), States.State1);
                        Assert.IsNull(sut.CheckTransitions(TransitionType.FixedUpdate));
                        Assert.IsNull(sut.CheckTransitions(TransitionType.LateUpdate));
                    }

                    [Test, AutoMoqData]
                    public void TransitionCheck_Returns_Null_On_NoTransitions(IStateMachine<States> stateMachine, StateData someData)
                    {
                        var sut = new StatefulTransitions<States, StateData> { };

                        sut.SetDataContainer(new StatefulContainer<StateData>(someData));
                        sut.Initialize(stateMachine);

                        Assert.IsNull(sut.CheckTransitions(TransitionType.Update));
                        Assert.IsNull(sut.CheckTransitions(TransitionType.FixedUpdate));
                        Assert.IsNull(sut.CheckTransitions(TransitionType.LateUpdate));
                    }

                    [Test, AutoMoqData]
                    public void TransitionCheck_ShouldRespect_TransitionOrder(
                        IStateMachine<States> stateMachine,
                        StateData someData,
                        TransitionType transitionType)
                    {
                        var sut = new StatefulTransitions<States, StateData>
                    {
                        {_ => States.State1, transitionType},
                        {_ => States.State2, transitionType},
                    };

                        sut.SetDataContainer(new StatefulContainer<StateData>(someData));
                        sut.Initialize(stateMachine);

                        Assert.AreEqual(sut.CheckTransitions(transitionType), States.State1);
                    }

                    [Test]
                    [InlineAutoMoqData(-1, States.State1)]
                    [InlineAutoMoqData(2, States.State2)]
                    [InlineAutoMoqData(3, null)]
                    public void TransitionCheck_SimpleForm_Returns_CorrectState(
                        int num,
                        States? expectedResult,
                        TransitionType transitionType,
                        IStateMachine<States> stateMachine,
                        StateData someData)
                    {
                        var sut = new StatefulTransitions<States, StateData>
                    {
                        {_ => num < 0, States.State1, transitionType},
                        {_ => num % 2 == 0, States.State2, transitionType},
                    };

                        sut.SetDataContainer(new StatefulContainer<StateData>(someData));
                        sut.Initialize(stateMachine);

                        Assert.AreEqual(sut.CheckTransitions(transitionType), expectedResult);
                    }

                    [Test]
                    [InlineAutoMoqData(-1, States.State1)]
                    [InlineAutoMoqData(2, States.State2)]
                    [InlineAutoMoqData(3, States.State3)]
                    [InlineAutoMoqData(400, null)]
                    public void TransitionCheck_Mixed_Returns_CorrectState(
                        int num,
                        States? expectedResult,
                        TransitionType transitionType,
                        IStateMachine<States> stateMachine,
                        StateData someData)
                    {
                        var sut = new StatefulTransitions<States, StateData>
                    {
                        {_ => num < 0 ? (States?)States.State1 : null, transitionType},
                        {_ => {
                            if (num > 100)
                            {
                                return null;
                            }
                            return num % 2 == 0 ? States.State2 : States.State3;
                        }, transitionType},
                        {
                            _ => {
                                var someNum = 50;
                                switch (someNum)
                                {
                                    case 1:
                                        return States.State1;
                                    case 2:
                                        return States.State2;
                                    case 3:
                                        return States.State3;
                                    default:
                                        return null;
                                }
                            }
                        }
                    };

                        sut.SetDataContainer(new StatefulContainer<StateData>(someData));
                        sut.Initialize(stateMachine);

                        Assert.AreEqual(sut.CheckTransitions(transitionType), expectedResult);
                    }

                    [Test]
                    [InlineAutoMoqData(Events.Event1, States.State1)]
                    [InlineAutoMoqData(Events.Event2, null)]
                    public void TransitionCheck_Events_SuperSimpleForm_Returns_CorrectState(
                        Events fsmEventToTrigger,
                        States? expectedState,
                        IStateMachine<States> stateMachine,
                        StateData someData)
                    {
                        var sut = new StatefulTransitions<States, Events, StateData> { { States.State1, Events.Event1 } };

                        sut.SetDataContainer(new StatefulContainer<StateData>(someData));
                        sut.Initialize(stateMachine);

                        Assert.AreEqual(sut.CheckEventTransitions(fsmEventToTrigger), expectedState);
                    }

                    [Test]
                    [InlineAutoMoqData(Events.Event1, true, States.State1)]
                    [InlineAutoMoqData(Events.Event1, false, null)]
                    [InlineAutoMoqData(Events.Event2, true, null)]
                    [InlineAutoMoqData(Events.Event2, false, null)]
                    public void TransitionCheck_Events_SimpleForm_Returns_CorrectState(
                        Events fsmEventToTrigger,
                        bool conditionShouldPass,
                        States? expectedState,
                        IStateMachine<States> stateMachine,
                        StateData someData)
                    {
                        var sut = new StatefulTransitions<States, Events, StateData> { {
                        _ => conditionShouldPass,
                        States.State1,
                        Events.Event1
                    } };

                        sut.SetDataContainer(new StatefulContainer<StateData>(someData));
                        sut.Initialize(stateMachine);

                        Assert.AreEqual(sut.CheckEventTransitions(fsmEventToTrigger), expectedState);
                    }

                    [Test]
                    [InlineAutoMoqData(Events.Event1, 1, States.State1)]
                    [InlineAutoMoqData(Events.Event1, 2, States.State2)]
                    [InlineAutoMoqData(Events.Event1, 100, States.State3)]
                    [InlineAutoMoqData(Events.Event1, 0, null)]
                    [InlineAutoMoqData(Events.Event2, 1, null)]
                    [InlineAutoMoqData(Events.Event2, 0, null)]
                    public void TransitionCheck_Events_ComplexForm_Returns_CorrectState(
                        Events fsmEventToTrigger,
                        int num,
                        States? expectedState,
                        IStateMachine<States> stateMachine,
                        StateData someData)
                    {
                        var sut = new StatefulTransitions<States, Events, StateData> { {
                        _ => num switch
                        {
                            1 => States.State1,
                            2 => States.State2,
                            var x when x >= 3 => States.State3,
                            _ => null
                        },
                        Events.Event1
                    } };

                        sut.SetDataContainer(new StatefulContainer<StateData>(someData));
                        sut.Initialize(stateMachine);

                        Assert.AreEqual(sut.CheckEventTransitions(fsmEventToTrigger), expectedState);
                    }

                    [Test]
                    [InlineAutoMoqData(Events.Event1, 1, States.State1)]
                    [InlineAutoMoqData(Events.Event1, -1, States.State1)]
                    [InlineAutoMoqData(Events.Event2, 1, States.State3)]
                    [InlineAutoMoqData(Events.Event2, -4, States.State2)]
                    [InlineAutoMoqData(Events.Event2, -5, null)]
                    public void TransitionCheck_Events_Mixed_Returns_CorrectState(
                        Events fsmEventToTrigger,
                        int num,
                        States? expectedState,
                        IStateMachine<States> stateMachine,
                        StateData someData)
                    {
                        var sut = new StatefulTransitions<States, Events, StateData> {
                        {States.State1, Events.Event1},
                        {_ => num > 0, States.State2, Events.Event1}, // event1 should never return State2
                        {_ => num > 0, States.State3, Events.Event2},
                        {_ => num % 2 == 0 ? States.State2 : (States?)null, Events.Event2}
                    };

                        sut.SetDataContainer(new StatefulContainer<StateData>(someData));
                        sut.Initialize(stateMachine);

                        Assert.AreEqual(sut.CheckEventTransitions(fsmEventToTrigger), expectedState);
                    }
                }

                [Test, InlineAutoMoqData(1, 2)]
                public void Modified_Data_Returns_Expected(
                    int someData,
                    int newData,
                    IStateMachine<States> stateMachine)
                {
                    var sut = new StatefulTransitions<States, Events, int>
                    {
                        {(_, data) => data == someData, States.State1},
                        {(_, data) => data == newData, States.State2}
                    };

                    var statefulContainer = new StatefulContainer<int>(someData);
                    sut.SetDataContainer(statefulContainer);
                    sut.Initialize(stateMachine);

                    statefulContainer.ResetData();

                    Assert.AreEqual(sut.CheckTransitions(TransitionType.Update), States.State1);

                    statefulContainer.CurrentData = newData;

                    Assert.AreEqual(sut.CheckTransitions(TransitionType.Update), States.State2);
                }
            }
        }
    }
}
