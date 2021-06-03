# SUSMachine - a Simple Unity State Machine

A state machine implementation with a quick, easy and nice to use API - no reflection or string names, no need to make a million subclasses.

## A Quick Glance

```cs
enum States { Normal, Blocking }
enum Events { Attacked }

[SerializedField]
float blockTime;

private void Awake()
{
    var fsm = new StateMachine<States, Events>
    {
        AnyState =
        {
            OnEnter = fsm => Debug.Log("Entering state " + fsm.CurrentState),
            OnExit = fsm => Debug.Log("Exiting state " + fsm.CurrentState)
        },

        [States.Normal] =
        {
            OnUpdate = fsm => Debug.Log("Time idling: " + fsm.TimeInState),

            OnEvents = {
                [Events.Attacked] = _ => health--
            },

            Transitions = {
                // in this state, check in update for space press. enter blocking state if true
                {_ => Input.GetKeyDown(KeyCode.Space), States.Blocking}
            }
        },

        [States.Blocking] =
        {
            OnEvents = {
                [Events.Attacked] = _ => Debug.Log("Attack blocked")
            },

            Transitions = {
                {fsm => fsm.TimeInState >= blockTime, States.Normal}
            }
        }
    };

    // Initialize can be called at any time when you're ready -- not just in Awake()
    stateMachine.Initialize(States.Idle);
}

void OnCollisionEnter(Collision collision)
{
    stateMachine.TriggerEvent(Events.Attacked);
}

void OnDestroy()
{
    // Call this when you are done with the state machine -- often this means in OnDestroy()
    stateMachine.Close();
}
```

# Installing

In the unity package manager window, click `add package from git URL...` and paste the following:

`https://github.com/JamesYFC/SUSMachine.git?path=/Packages/com.kdmagical.susmachine`

Now just import the namespace and you're good to go.

```cs
using KDMagical.SUSMachine
```

# State Behaviours

A state behaviour contains all the actions to call when certain events happen, as well as the transitions.

The state machine is provided as the sole parameter when calling these actions.

The available actions are:

- `OnEnter`
- `OnExit`
- `OnUpdate`
- `OnFixedUpdate`
- `OnLateUpdate`
- `OnEvents` (when events are enabled)

A StateMachine can contain a state behaviour for each member of the states enum, plus `AnyState`, whose actions and transitions are called before any specific state.

```cs
var fsm = new StateMachine<States>
{
    AnyState =
    {
        //...
    },

    [States.State1] =
    {
        // ...
    },

    [States.State2] =
    {
        // ...
    },

    [States.State3] =
    {
        // ...
    },
}
```

## Events

State behaviours also support events for actions and transitions.

Simply add another generic parameter to `StateMachine` to enable this support.

```cs
// without events
var fsm = new StateMachine<States> { }

// with events
var fsm = new StateMachine<States, Events>
{
    [States.State1] = {
        OnEvents = {
            [Events.Event1] = _ => Debug.Log("Event1 Triggered!")
        }
    }
}
```

## Transitions

### Automatic

Automatic transitions are functions that run on a specified update loop (`Update`, `FixedUpdate` or `LateUpdate`) after behaviour actions trigger, checking for if the state machine should automatically switch to another state.

You can setup as many automatic transitions as you want on a specific state behaviour.

There are two ways to setup automatic transitions:

#### _Simple Syntax_

`{PredicateCondition, State, TransitionType}`

```cs
var stateMachine = new StateMachine<States>
{
    [States.Idle] =
    {
        Transitions =
        {
            {
                // if this returns true, enter the state specified.
                _ => Input.GetKeyDown(KeyCode.Space),
                // the state to enter if the predicate returned true.
                States.Jumping,
                // the update loop to run on. Can be omitted as Update is the default.
                TransitionType.Update
            }
        }
    }
}
```

#### _Complex Syntax_

`{TransitionFunction, TransitionType}`

Allows you to define a function where you can return any state or `null`, allowing you to set up several conditions in the same place.

The following example is a simple switch case, but the logic in this function can be as complex as you need.

```cs
Transitions =
{
    {
        _ => {
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
    },
    // this last parameter is not necessary as it will default to TransitionType.Update if omitted
    TransitionType.Update
}
```

In some cases, such as below, the compiler can't figure out that the returned type is `States?`.

If this happens you simply need to cast one of the returns like so:

```cs
[States.Idle] =
{
    Transitions =
    {
        {
            _ => someNum < 0
                ? States.State1
                : (States?)null
        }
    }
}
```

### Event Transitions

There are several ways to write event transitions.

These are only run when the event is triggered, directly after any event actions are called.

#### _Direct_

`{State, Event}`

Enters the specified state when the event is called.

```cs
Transitions =
{
    { States.State1, Events.Event1 }
}
```

#### _Simple_

`{PredicateCondition, State, Event}`

```cs
Transitions =
{
    // when the event is triggered, the condition is called. Enters State1 if the condition is true.
    { someNum > 0, States.State1, Events.Event1 }
}
```

#### _Complex_

`{TransitionFunction, Event}`

Allows you to define a function where you can return any state or `null`, allowing you to set up several conditions in the same place.

```cs
Transitions =
{
    _ => someNum switch
    {
        1 => States.State1,
        2 => States.State2,
        var x when x >= 3 => States.State3,
        _ => null
    },
    Events.Event1
}
```

###

### Order

As soon as the first automatic transition in a particular update loop returns a positive result, the state machine will switch its state to that result.

Therefore, the order in which you set your transitions matters.

Consider the following:

```cs
var stateMachine = new StateMachine<States>
{
    [States.Jumping] =
    {
        Transitions =
        {
            {
                _ => {
                    Debug.Log("first check");
                    return GetJumpHeight() > limit;
                },
                States.State1
            },
            {
                fsm => {
                    Debug.Log("second check");
                    return fsm.TimeInState > jumpTime;
                },
                States.State2
            }
        }
    }
}
```

If the return value of `GetJumpHeight()` rises above the limit before, or in the same frame, as `jumpTime` is surpassed, then the second check will not occur and the state will be set to `States.State1`.

### Manual

The built-in event and transitions support should cover practically all use cases, but a manual `SetState` method is provided just in case you need it.

```cs
void StateCheck()
{
    if (someCondition)
    {
        stateMachine.SetState(States.State1);
    }
}
```

This method should **not** be called within state callback actions, as it can cause confusing results or even a stack overflow!

```cs
var fsm = new StateMachine<States>
{
    AnyState =
    {
        OnEnter = fsm => fsm.SetState(States.State2)
    }
}

fsm.Initialize(States.State1); // stack overflow! AnyState.OnEnter called infinitely!
```

## Splitting Out

If you find that your functions get too large to be written in lambda syntax, or for any other reason, you can write them elsewhere -- as long as they have the matching signature `void MyFunc(IStateMachine<MyStates> fsm)`.

```cs
// in state machine init...
[States.SomeState] = {
    OnEnter = SomeFunc
}

void SomeFunc(IStateMachine<States> stateMachine)
{
    // ...
}
```
