# SUSMachine - a Simple Unity State Machine

A state machine implementation with a quick, easy and nice to use API - no reflection or string names, no need to inherit anything.

Supports Unity 2020.2 and up.

## A Quick Glance

```cs
enum States { Normal, Blocking }
enum Events { Attacked }

int lives = 3;

[SerializeField]
float blockTime = 2;

StateMachine<States, Events> fsm;

private void Awake()
{
    fsm = new StateMachine<States, Events>
    {
        AnyState =
        {
            OnEnter = fsm => Debug.Log($"Entering state {fsm.CurrentState}"),
            
            OnExit = fsm => Debug.Log($"Exiting state {fsm.CurrentState} to {fsm.NextState}")
        },

        [States.Normal] =
        {
            OnUpdate = fsm => Debug.Log($"Time idling: {fsm.TimeInState}"),

            [Events.Attacked] = _ => lives--,

            Transitions = {
                // in this state, check in update for space press. enter blocking state if true
                {_ => Input.GetKeyDown(KeyCode.Space), States.Blocking}
            }
        },

        [States.Blocking] =
        {
            [Events.Attacked] = _ => Debug.Log("Attack blocked"),

            Transitions = {
                {fsm => fsm.TimeInState >= blockTime, States.Normal}
            }
        }
    };

    /// Initialize can be called at any time when you're ready -- not just in Awake().
    /// By supplying this gameObject into the second parameter,
    /// the state machine will autmatically Close() when the gameObject is destroyed.
    /// Note: Initialization calls Enter() and Closing calls Exit().
    stateMachine.Initialize(States.Idle, this.gameObject);
}

void OnCollisionEnter(Collision collision)
{
    stateMachine.TriggerEvent(Events.Attacked);
}
```

# Installing

In the unity package manager window, click `add package from git URL...` and paste the following:

`https://github.com/JamesYFC/SUSMachine.git?path=/Packages/com.kdmagical.susmachine#release`

Now just import the namespace and you're good to go.

```cs
using KDMagical.SUSMachine;
```

## Updating

Unity currently doesn't support updating upm packages from git sources as of 2020.3 (i.e. you will not be notified by version updates, and the update button will not be available.)

However, you can simply reinstall the package by following the installation step above again.

The existing version of this package should be overwritten by the new version.

# Noteworthy Changes

- **v0.9.0** - In transitions with events, the event parameter is now first in the order.
  e.g. `{States.State1, Events.Event1}` is now `{Events.Event1, States.State1}`

# State Objects

A state object contains all the actions to call when certain events happen, as well as the transitions.

The state machine is provided as the sole parameter when calling these actions.

The available actions are:

- `OnEnter`
- `OnExit`
- `OnUpdate`
- `OnFixedUpdate`
- `OnLateUpdate`
- `[EventEnum.EventName]` (when events are enabled)

A StateMachine can contain a state object for each member of the states enum, plus `AnyState`, whose actions are called before any specific state's. AnyState's transitions take lower priority than specific state object's transitions.

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

State objects also support events for actions and transitions.

Simply add another generic parameter to `StateMachine` to enable this support.

```cs
// without events
var fsm = new StateMachine<States> { };

// with events
var fsm = new StateMachine<States, Events>
{
    [States.State1] =
    {
        [Events.Event1] = _ => Debug.Log("Event1 Triggered!")
    }
};
```

You can then trigger these events with `fsm.TriggerEvent(TEvents eventToTrigger)`.

## Stateful Data

To keep track of and modify variables within a state object, use the `Stateful<TStates, TData>` or `Stateful<TStates, TEvents, TData>` classes.

`TData` must be a `struct` type.

When using this class, your actions and transition functions get more parameters related to the data being tracked.

```cs
var fsm =
{
    [States.State1] = new Stateful<States, (string name, int count)>
    {
        // names are optional here -- ("Hello", 0) does the same thing.
        InitialData = (name: "Hello", count: 0),

        OnEnter = (_, data, modify) =>
        {
            // deconstruct syntax
            var (name, count) = data;

            Debug.Log($"Name: {name}, Count: {count}");

            data.count++;

            modify(data);
        }

        Transitions =
        {
            { // transitions to State2 on the first frame where count reaches 2
                (_, data) => data.count >= 2,
                States.State2
            }
        }
    }
};

var fsm = new Stateful<States, int>
{
    [States.State1] = new Stateful<States, Events, int>
    {
        // as data is a struct, omitting InitialData will mean it is set to its default value.
        OnEnter = (_, data, __) => Debug.Log("data = " + data) // data = 0
    }
};
```

## Transitions

### Automatic

Automatic transitions are functions that run on a specified update loop (`Update`, `FixedUpdate` or `LateUpdate`) after behaviour actions trigger, checking for if the state machine should automatically switch to another state.

You can setup as many automatic transitions as you want on a specific state object.

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

When using `Stateful`, the `PredicateCondition` can also be written in the form `(stateMachine, data) => bool`.

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

When using `Stateful`, the `TransitionFunction` can also be written in the form `(stateMachine, data) => States?`.

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

`{Event, State}`

When the event is called, enters the specified state.

```cs
Transitions =
{
    { Events.Event1, States.State1 }
}
```

#### _Simple_

`{Event, PredicateCondition, State}`

```cs
Transitions =
{
    // when the event is triggered, the condition is called. Enters State1 if the condition is true.
    { Events.Event1, _ => someNum > 0, States.State1 }
}
```

When using `Stateful`, the `PredicateCondition` can also be written in the form `(stateMachine, data) => bool`.

#### _Complex_

`{Event, TransitionFunction}`

Allows you to define a function where you can return any state or `null`, allowing you to set up several conditions in the same place.

```cs
Transitions =
{
    Events.Event1,
    _ => someNum switch
    {
        1 => States.State1,
        2 => States.State2,
        var x when x >= 3 => States.State3,
        _ => null
    }
}
```

When using `Stateful`, the `TransitionFunction` can also be written in the form `(stateMachine, data) => States?`.

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

The built-in event and transitions support should cover most use cases, but a manual `SetState` method is also provided.

```cs
void StateCheck()
{
    if (someCondition)
    {
        stateMachine.SetState(States.State1);
    }
}
```

Be careful when using this in `OnEnter` and `OnExit`, as improper use can cause a stack overflow!

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

If you find that your functions get too large to be written in lambda syntax, or for any other reason, you can write them elsewhere -- as long as they have the matching signature.

```cs
// in state machine init...
[States.SomeState] = {
    OnEnter = SomeFunc
}

void SomeFunc(IStateMachine<States> stateMachine) { }
```

```cs
// in state machine init...
[States.SomeState] = Stateful<States, MyStruct>
{
    OnEnter = SomeFunc
}

void SomeFunc(IStateMachine<States> stateMachine, MyStruct data) { }
```

# Manual Initialization & Closing

State machines need to call `Close()` when their use is finished.

This is done for you if when you specify the second parameter in `Initialize()`.

A `MonoBehaviour` will be created that calls `Close()` on the state machine when the specified `GameObject` is destroyed:

```cs
Initialize(T initialState, GameObject closeOnDestroy)
```

However, you may omit the second parameter if you want to call `Close()` manually.

```cs
fsm.Initialize(States.State1);

// elsewhere, e.g. in OnDestroy
fsm.Close();
```
