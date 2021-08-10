using NUnit.Framework;
using Moq;
using System;

namespace KDMagical.SUSMachine.Tests
{
    public partial class SUSMachineTests
    {
        internal class StateObjectTests
        {
            internal class Stateless
            {
                [Test, AutoMoqData]
                public void StateObject_Calls_Actions(IStateMachine<States> stateMachine)
                {
                    var actions = new MockStateActions();
                    var (enter, exit, update, fixedUpdate, lateUpdate) = actions;

                    var sut = new Stateless<States>
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
                public void StateObject_Events_Calls_Actions(IStateMachine<States> stateMachine)
                {
                    var (enter, exit, update, fixedUpdate, lateUpdate) = new MockStateActions();
                    var (event1, event2, event3) = new MockEventActions();

                    var sut = new Stateless<States, Events>
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
                public void StateObject_Events_Returns_Transition(
                    Events eventToTrigger,
                    States? expectedState,
                    IStateMachine<States> stateMachine
                )
                {
                    var sut = new Stateless<States, Events>
                    {
                        Transitions =
                        {
                            {States.State1, Events.Event1}
                        }
                    };

                    Assert.AreEqual(sut.TriggerEventAndGetTransition(eventToTrigger), expectedState);
                }
            }

            internal class Stateful
            {
                internal class Eventless
                {
                    [Test, AutoMoqData]
                    public void StateObject_Calls_Actions_With_Correct_Params(
                        StateData initialData,
                        IStateMachine<States> stateMachine)
                    {
                        var actions = new MockStatefulActions<StateData>();
                        var (enter, exit, update, fixedUpdate, lateUpdate) = actions;

                        var sut = new Stateful<States, StateData>
                        {
                            InitialData = initialData,
                            OnEnter = enter.Object,
                            OnExit = exit.Object,
                            OnUpdate = update.Object,
                            OnFixedUpdate = fixedUpdate.Object,
                            OnLateUpdate = lateUpdate.Object
                        };

                        sut.Initialize(stateMachine);

                        sut.DoEnter();

                        enter.Verify(f => f(stateMachine, initialData, It.IsAny<Action<StateData>>()), Times.Once);
                        exit.VerifyNoOtherCalls();
                        update.VerifyNoOtherCalls();
                        fixedUpdate.VerifyNoOtherCalls();
                        lateUpdate.VerifyNoOtherCalls();

                        sut.DoExit();

                        enter.VerifyNoOtherCalls();
                        exit.Verify(f => f(stateMachine, initialData, It.IsAny<Action<StateData>>()), Times.Once);
                        update.VerifyNoOtherCalls();
                        fixedUpdate.VerifyNoOtherCalls();
                        lateUpdate.VerifyNoOtherCalls();

                        sut.DoUpdate();

                        enter.VerifyNoOtherCalls();
                        exit.VerifyNoOtherCalls();
                        update.Verify(f => f(stateMachine, initialData, It.IsAny<Action<StateData>>()), Times.Once);
                        fixedUpdate.VerifyNoOtherCalls();
                        lateUpdate.VerifyNoOtherCalls();

                        sut.DoFixedUpdate();

                        enter.VerifyNoOtherCalls();
                        exit.VerifyNoOtherCalls();
                        update.VerifyNoOtherCalls();
                        fixedUpdate.Verify(f => f(stateMachine, initialData, It.IsAny<Action<StateData>>()), Times.Once);
                        lateUpdate.VerifyNoOtherCalls();

                        sut.DoLateUpdate();

                        enter.VerifyNoOtherCalls();
                        exit.VerifyNoOtherCalls();
                        update.VerifyNoOtherCalls();
                        fixedUpdate.VerifyNoOtherCalls();
                        lateUpdate.Verify(f => f(stateMachine, initialData, It.IsAny<Action<StateData>>()), Times.Once);
                    }

                    [Test]
                    [InlineAutoMoqData(Events.Event1, States.State1)]
                    [InlineAutoMoqData(Events.Event2, null)]
                    public void StateObject_Events_Returns_Transition(
                        Events eventToTrigger,
                        States? expectedState,
                        IStateMachine<States> stateMachine
                    )
                    {
                        var sut = new Stateless<States, Events>
                        {
                            Transitions =
                        {
                            {States.State1, Events.Event1}
                        }
                        };

                        Assert.AreEqual(sut.TriggerEventAndGetTransition(eventToTrigger), expectedState);
                    }

                    [Test, InlineAutoMoqData(1, 2)]
                    public void Modify_Function_And_Exit_Modifies_Data_Correctly(
                        int initialData,
                        int newData,
                        IStateMachine<States> stateMachine)
                    {
                        var sut = new Stateful<States, int>
                        {
                            InitialData = initialData,

                            OnEnter = (fsm, data, modify) => modify(newData)
                        };

                        sut.Initialize(stateMachine);

                        Assert.AreEqual(initialData, sut.CurrentData);

                        sut.DoEnter();

                        Assert.AreEqual(newData, sut.CurrentData);

                        sut.DoExit();

                        Assert.AreEqual(initialData, sut.CurrentData);
                    }

                    [Test, InlineAutoMoqData(1, 2)]
                    public void Modified_Data_Received_By_Callback(
                        int initialData,
                        int newData,
                        Mock<StatefulAction<States, int>> actionMock,
                        IStateMachine<States> stateMachine)
                    {
                        var sut = new Stateful<States, int>
                        {
                            InitialData = initialData,

                            OnEnter = (_, data, modify) =>
                            {
                                Assert.AreEqual(initialData, data);
                                modify(newData);
                            },

                            OnUpdate = actionMock.Object
                        };

                        sut.Initialize(stateMachine);

                        sut.DoEnter();

                        Assert.AreEqual(newData, sut.CurrentData);

                        sut.DoUpdate();

                        actionMock.Verify(f => f(stateMachine, newData, It.IsAny<Action<int>>()));
                    }

                    [Test, InlineAutoMoqData(1, 2)]
                    public void Exit_Resets_Data(
                        int initialData,
                        int newData,
                        IStateMachine<States> stateMachine)
                    {
                        var sut = new Stateful<States, int>
                        {
                            InitialData = initialData,

                            OnEnter = (_, data, modify) => modify(newData),
                        };

                        sut.Initialize(stateMachine);

                        sut.DoEnter();

                        Assert.AreEqual(newData, sut.CurrentData);

                        sut.DoExit();

                        Assert.AreEqual(initialData, sut.CurrentData);
                    }
                }

                internal class Eventful
                {
                    [Test, AutoMoqData]
                    public void StateObject_Events_Calls_Actions_With_Correct_Params(
                        StateData initialData,
                        IStateMachine<States> stateMachine)
                    {
                        var (enter, exit, update, fixedUpdate, lateUpdate) = new MockStatefulActions<StateData>();
                        var (event1, event2, event3) = new MockStatefulEventActions<StateData>();

                        var sut = new Stateful<States, Events, StateData>
                        {
                            InitialData = initialData,

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

                        enter.Verify(f => f(stateMachine, initialData, It.IsAny<Action<StateData>>()), Times.Once);
                        exit.VerifyNoOtherCalls();
                        update.VerifyNoOtherCalls();
                        fixedUpdate.VerifyNoOtherCalls();
                        lateUpdate.VerifyNoOtherCalls();

                        sut.DoExit();

                        enter.VerifyNoOtherCalls();
                        exit.Verify(f => f(stateMachine, initialData, It.IsAny<Action<StateData>>()), Times.Once);
                        update.VerifyNoOtherCalls();
                        fixedUpdate.VerifyNoOtherCalls();
                        lateUpdate.VerifyNoOtherCalls();

                        sut.DoUpdate();

                        enter.VerifyNoOtherCalls();
                        exit.VerifyNoOtherCalls();
                        update.Verify(f => f(stateMachine, initialData, It.IsAny<Action<StateData>>()), Times.Once);
                        fixedUpdate.VerifyNoOtherCalls();
                        lateUpdate.VerifyNoOtherCalls();

                        sut.DoFixedUpdate();

                        enter.VerifyNoOtherCalls();
                        exit.VerifyNoOtherCalls();
                        update.VerifyNoOtherCalls();
                        fixedUpdate.Verify(f => f(stateMachine, initialData, It.IsAny<Action<StateData>>()), Times.Once);
                        lateUpdate.VerifyNoOtherCalls();

                        sut.DoLateUpdate();

                        enter.VerifyNoOtherCalls();
                        exit.VerifyNoOtherCalls();
                        update.VerifyNoOtherCalls();
                        fixedUpdate.VerifyNoOtherCalls();
                        lateUpdate.Verify(f => f(stateMachine, initialData, It.IsAny<Action<StateData>>()), Times.Once);

                        sut.TriggerEventAndGetTransition(Events.Event1);

                        event1.Verify(f => f(stateMachine, initialData, It.IsAny<Action<StateData>>()), Times.Once);
                        event2.VerifyNoOtherCalls();
                        event3.VerifyNoOtherCalls();

                        sut.TriggerEventAndGetTransition(Events.Event2);

                        event1.VerifyNoOtherCalls();
                        event2.Verify(f => f(stateMachine, initialData, It.IsAny<Action<StateData>>()), Times.Once);
                        event3.VerifyNoOtherCalls();

                        sut.TriggerEventAndGetTransition(Events.Event3);

                        event1.VerifyNoOtherCalls();
                        event2.VerifyNoOtherCalls();
                        event3.Verify(f => f(stateMachine, initialData, It.IsAny<Action<StateData>>()), Times.Once);
                    }

                    [Test, InlineAutoMoqData(1, 2)]
                    public void Modified_Data_Received_By_Callback(
                        int initialData,
                        int newData,
                        Events someEvent,
                        Mock<StatefulAction<States, int>> actionMock,
                        IStateMachine<States> stateMachine)
                    {
                        var sut = new Stateful<States, Events, int>
                        {
                            InitialData = initialData,

                            OnEnter = (_, data, modify) =>
                            {
                                Assert.AreEqual(initialData, data);
                                modify(newData);
                            },

                            [someEvent] = actionMock.Object
                        };

                        sut.Initialize(stateMachine);

                        sut.DoEnter();

                        Assert.AreEqual(newData, sut.CurrentData);

                        sut.TriggerEventAndGetTransition(someEvent);

                        actionMock.Verify(f => f(stateMachine, newData, It.IsAny<Action<int>>()));
                    }

                    [Test, InlineAutoMoqData(1, 2)]
                    public void Exit_Resets_Data(
                        int initialData,
                        int newData,
                        IStateMachine<States> stateMachine)
                    {
                        var sut = new Stateful<States, Events, int>
                        {
                            InitialData = initialData,

                            OnEnter = (_, data, modify) => modify(newData),
                        };

                        sut.Initialize(stateMachine);

                        sut.DoEnter();

                        Assert.AreEqual(newData, sut.CurrentData);

                        sut.DoExit();

                        Assert.AreEqual(initialData, sut.CurrentData);
                    }
                }
            }
        }
    }
}