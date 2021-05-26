using UnityEngine;
using KDMagical.SUSMachine;

public class Test : MonoBehaviour
{
    private enum States { Idle, Jumping }

    private float jumpTime = 5f;

    private StateMachine<States> stateMachine;

    private void Awake()
    {
        stateMachine = new StateMachine<States>
        {
            [States.Idle] =
            {
                OnEnter = _ => Debug.Log("entering idle"),
                OnUpdate = fsm => Debug.Log("time since idle: " + fsm.TimeInState),
                OnExit = _ => Debug.Log("exiting idle"),

                AutoTransitions =
                {
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

        stateMachine.Initialize(States.Idle);
    }
}
