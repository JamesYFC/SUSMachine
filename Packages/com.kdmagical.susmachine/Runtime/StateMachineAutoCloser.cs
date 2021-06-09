using UnityEngine;

namespace KDMagical.SUSMachine
{
    public class StateMachineAutoCloser : MonoBehaviour
    {
        private IStateMachine stateMachine;

        public void Initialize(IStateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        private void OnDestroy()
        {
            if (stateMachine != null)
            {
                stateMachine.Close();
            }
        }
    }
}