using System.Collections.Generic;
using UnityEngine;

namespace KDMagical.SUSMachine
{
    public interface IStateMachineManager
    {
        void Register(IStateMachine stateMachine);
        void Deregister(IStateMachine stateMachine);
    }

    public class StateMachineManager : SingletonMonoBehaviour<StateMachineManager>, IStateMachineManager
    {
        private List<IStateMachine> stateMachines = new List<IStateMachine>();

        public void Register(IStateMachine stateMachine)
            => stateMachines.Add(stateMachine);

        public void Deregister(IStateMachine stateMachine)
            => stateMachines.Remove(stateMachine);

        private void Awake()
        {
            Debug.Log("State Machine Manager created.");
        }

        private void Update()
        {
            foreach (var stateMachine in stateMachines)
            {
                stateMachine.DoUpdate();
            }
        }

        private void FixedUpdate()
        {
            foreach (var stateMachine in stateMachines)
            {
                stateMachine.DoFixedUpdate();
            }
        }

        private void LateUpdate()
        {
            foreach (var stateMachine in stateMachines)
            {
                stateMachine.DoLateUpdate();
            }
        }
    }
}