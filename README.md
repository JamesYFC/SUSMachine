# SUSMachine - a Simple Unity State Machine

A state machine implementation with two goals in mind:

- Quick, easy and nice to use API - no reflection or string names, no need to make a million subclasses

- Describe a lot of behaviour and transitions without writing much code

## A Quick Glance

```cs
float jumpTime = 5f;
enum States { Idle, Jumping }
StateMachine<States> stateMachine;

void Awake()
{
    stateMachine = new StateMachine<States>
    {
        [States.Idle] =
        {
            OnEnter = _ => Debug.Log("entering idle"),

            // the StateMachine<States> is passed in as the sole argument
            OnUpdate = fsm => Debug.Log("time since idle: " + fsm.TimeInState),

            OnExit = _ => Debug.Log("exiting idle"),

            AutoTransitions =
            {
                // check for spacebar press on update. on true, switch to desired state
                {
                    _ => Input.GetKeyDown(KeyCode.Space),
                    States.Jumping
                }
            }
        },

        [States.Jumping] =
        {
            OnEnter = _ => Debug.Log("entering jumping"),
            OnExit = _ => Debug.Log("exiting jumping"),

            AutoTransitions =
            {
                {
                    fsm => fsm.TimeInState > jumpTime,
                    States.Idle
                }
            }
        }
    };

    // Initialize can be called at any time when you're ready -- not just in Awake()
    stateMachine.Initialize(States.Idle);
}
```

## Installing

In the unity package manager window, click `add package from git URL...` and paste the following:

`https://github.com/JamesYFC/SUSMachine.git?path=/Packages/com.kdmagical.susmachine`

Now just import the namespace and you're good to go.

```cs
using KDMagical.SUSMachine
```

## State Behaviours

A state behaviour contains all the actions to call when certain events happen, as well as the automatic transition checks.

The state machine is provided as the sole parameter when calling these actions.

The available actions are:

- `OnEnter`
- `OnExit`
- `OnUpdate`
- `OnFixedUpdate`
- `OnLateUpdate`

## Automatic Transitions

Automatic transitions are functions that run on a specified update loop (`Update`, `FixedUpdate` or `LateUpdate`) before any behaviour actions trigger, checking for if the state machine should automatically switch to another state.

You can setup as many automatic transitions as you want on a specific state behaviour.

There are two ways to setup automatic transitions:

### Simple Syntax

The simple syntax is in the format `{PredicateCondition, NextState, TransitionType}`.

- The predicate condition is simply a function that returns `true` if we should transition to NextState, and `false` otherwise.

- NextState is the enum state that, when the condition resolves true, the state machine should automatically switch to.

- TransitionType (defaults to `TransitionType.Update`) specifies what life cycle loop to make our transition check on.

```cs
var stateMachine = new StateMachine<States>
{
    [States.Idle] =
    {
        AutoTransitions =
        {
            // the last parameter is not necessary as it defaults to TransitionType.Update if omitted
            {_ => Input.GetKeyDown(KeyCode.Space), States.Jumping, TransitionType.Update}
        }
    }
}
```

### Complex Syntax

This syntax allows you to define a function where you can return any state or `null`, allowing you to set up several conditions in the same place.

The following example is a simple switch case, but the logic in this function can be as complex as you need.

```cs
var stateMachine = new StateMachine<States>
{
    [States.Idle] =
    {
        AutoTransitions =
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
    }
}
```

In some cases, such as below, the compiler can't figure out that the returned type is `States?`.

You simply need to cast one of the returns like so:

```cs
[States.Idle] =
{
    AutoTransitions =
    {
        {_ => someNum < 0 ? States.State1 : (States?)null}
    }
}
```

### Order

As soon as the first AutoTransition returns a positive result, the state machine will switch its state to that result.

Therefore, the order in which you set your transitions matters.

Consider the following:

```cs
var stateMachine = new StateMachine<States>
{
    [States.Jumping] =
    {
        AutoTransitions =
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

There are many cases where making checks in one of the update methods may be inconvenient or costly.

There is a manual `SetState` method for the cases where automatic transitions aren't worth it.

```cs
UnityEvent someEvent;
bool someCondition;

void Awake()
{
    someEvent.AddListener(StateCheck);
}

void StateCheck()
{
    if (someCondition)
    {
        stateMachine.SetState(States.State1);
    }
}
```
