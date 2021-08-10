using UnityEngine;
using KDMagical.SUSMachine;

public class Test : MonoBehaviour
{
    private enum States { Idle, Jumping, Blocking }

    private float jumpTime = 4f;
    private int blockFrames = 40;

    private StateMachine<States> stateMachine;

    private void Awake()
    {
        stateMachine = new StateMachine<States>
        {
            AnyState = {
                OnEnter = fsm => Debug.Log("entering " + fsm.CurrentState)
            },

            [States.Idle] =
            {
                //OnUpdate = fsm => Debug.Log("time since idle: " + fsm.TimeInState),
                OnExit = SomeFunc,

                Transitions =
                {
                    {
                        _ => Input.GetKeyDown(KeyCode.Space),
                        States.Jumping
                    },
                    {
                        _ => Input.GetKeyDown(KeyCode.Alpha1),
                        States.Blocking
                    }
                }
            },

            [States.Jumping] =
            {
                OnExit = _ => Debug.Log("exiting jumping"),

                Transitions =
                {
                    {
                        fsm => fsm.TimeInState > jumpTime,
                        States.Idle
                    }
                }
            },

            [States.Blocking] = new Stateful<States, (int frames, string someString)>
            {
                OnUpdate = (_, data, modify) =>
                {
                    Debug.Log($"frame: {data.frames}, str: {data.someString}");

                    data.frames++;
                    // every frame it ups count by 1
                    modify(data);
                },

                Transitions =
                {
                    {(_, data) => data.frames > blockFrames, States.Idle}
                }
            }
        };

        stateMachine.Initialize(States.Idle, gameObject);
    }

    private void SomeFunc(IStateMachine<States> stateMachine)
    {
        Debug.Log("SomeFunc Called");
    }

    private void OnDestroy()
    {
        stateMachine.Close();
    }

    #region api tests

    enum TestStates { A, B }
    enum TestEvents { X, Y }

    private void StatefulApiTest()
    {
        var fsm = new StateMachine<TestStates, TestEvents>
        {
            [TestStates.A] = new Stateful<TestStates, TestEvents, int>
            {
                OnEnter = (fsm, val, modify) => Debug.Log(val),
                [TestEvents.X] = (fsm, val, modify) => Debug.Log(val),

                Transitions =
                {
                    // complex omit data
                    {_ => TestStates.A},
                    // complex with data
                    {(fsm, data) => TestStates.A},
                    // simple omit data
                    {_ => true, TestStates.B, TestEvents.X},
                    // simple with data
                    {(_, data) => true, TestStates.B, TestEvents.X},
                    // super simple
                    {TestStates.B, TestEvents.Y}
                }
            }
        };
    }

    #endregion
}
