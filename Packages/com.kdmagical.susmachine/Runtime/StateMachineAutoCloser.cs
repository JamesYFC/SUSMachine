using UnityEngine;

namespace KDMagical.SUSMachine
{
    public class StateMachineAutoCloser : MonoBehaviour
    {
        private IStateMachine stateMachine;
        public bool CallExit { get; set; }

        public void Initialize(IStateMachine stateMachine, bool callExit = true)
        {
            this.stateMachine = stateMachine;
            CallExit = callExit;
        }

        private void OnDestroy()
        {
            if (stateMachine != null)
            {
                stateMachine.Close(CallExit);
            }
        }
    }
}