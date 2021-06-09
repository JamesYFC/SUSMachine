using UnityEngine;
using KDMagical.SUSMachine;

public class Test : MonoBehaviour
{
    private enum States { Idle, Jumping }

    private float jumpTime = 4f;

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
                OnUpdate = fsm => Debug.Log("time since idle: " + fsm.TimeInState),
                OnExit = SomeFunc,

                Transitions =
                {
                    {
                        _ => Input.GetKeyDown(KeyCode.Space),
                        States.Jumping
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
