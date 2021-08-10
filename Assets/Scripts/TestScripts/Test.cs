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
                InitialData =
                (
                    frames: 0,
                    someString: ""
                ),

                OnUpdate = (_, data, modify) =>
                {
                    // every frame it ups count by 1
                    modify((frames: data.frames + 1, someString: data.someString));
                    Debug.Log("data: " + data.frames);
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
}
