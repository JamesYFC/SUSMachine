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
                        States.Jumping,
                        _ => Input.GetKeyDown(KeyCode.Space)
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
                        States.Idle,
                        fsm => fsm.TimeInState > jumpTime
                    }
                }
            }
        };

        stateMachine.Initialize(States.Idle);
    }
}
