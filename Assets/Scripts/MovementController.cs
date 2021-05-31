using KDMagical.SUSMachine;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D rb;
    [SerializeField]
    private float speed = 5;
    [SerializeField]
    private float dodgeTime = .5f;
    [SerializeField]
    private float dodgeDistance = 1;

    private enum MovementStates { Normal, Dodging, Disabled }
    private enum MovementEvents { Knockdown }

    private StateMachine<MovementStates, MovementEvents> stateMachine;

    private void Awake()
    {
        Vector2 inputVecNorm = new Vector2();
        (Vector2 dir, Vector2 startPos, Vector2 endPos)? dodgeData = null;

        stateMachine = new StateMachine<MovementStates, MovementEvents>
        {
            [MovementStates.Normal] =
            {
                OnUpdate = _ =>
                    inputVecNorm = new Vector2(
                        Input.GetAxisRaw("Horizontal"),
                        Input.GetAxisRaw("Vertical")
                    ).normalized,

                OnFixedUpdate = _ =>
                    rb.MovePosition(
                        rb.position +
                        inputVecNorm * speed * Time.fixedDeltaTime
                    ),

                OnEvents = {
                    [MovementEvents.Knockdown] = _ => Debug.Log("knocked down")
                },

                Transitions = {
                    {_ => Input.GetAxisRaw("Jump") > 0, MovementStates.Dodging},
                    // complex new
                    {_ => MovementStates.Normal}
                }
            },

            [MovementStates.Dodging] =
            {
                OnEnter = _ => dodgeData = (
                    inputVecNorm,
                    rb.position,
                    rb.position + inputVecNorm * dodgeDistance
                ),

                OnFixedUpdate = fsm => {
                    if (dodgeData == null)
                    {
                        Debug.LogError("dodge data is null for some reason");
                        return;
                    }

                    var data = dodgeData.Value;
                    rb.MovePosition(
                        Vector2.Lerp(data.startPos, data.endPos, fsm.TimeInState / dodgeTime)
                    );
                },

                OnExit = _ => dodgeData = null,

                Transitions = {
                    { fsm => fsm.TimeInState >= dodgeTime, MovementStates.Normal}
                }
            }
        };

        stateMachine.Initialize(MovementStates.Normal);
    }

    private void OnDisable()
    {
        stateMachine.Close();
    }
}
