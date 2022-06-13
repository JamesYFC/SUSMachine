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
        private readonly List<IStateMachine> stateMachines = new List<IStateMachine>();
        public int Count => stateMachines.Count;
        private readonly HashSet<IStateMachine> stateMachinesToRemove = new HashSet<IStateMachine>();
        // todo: this is a naive starting estimate. potentiallly gauge how often to trigger cleanup
        public static int CleanupThreshold { get; set; } = 4;

        public void Register(IStateMachine stateMachine)
            => stateMachines.Add(stateMachine);

        public void Deregister(IStateMachine stateMachine)
        {
            stateMachinesToRemove.Add(stateMachine);
        }

        private void Awake()
        {
            Debug.Log("State Machine Manager created.");
        }

        private void Update()
        {
            for (int i = 0; i < stateMachines.Count; i++)
            {
                IStateMachine stateMachine = stateMachines[i];

                if (stateMachinesToRemove.Contains(stateMachine))
                    continue;

                stateMachine.DoUpdate();
            }

            CleanupCheck();
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < stateMachines.Count; i++)
            {
                IStateMachine stateMachine = stateMachines[i];

                if (stateMachinesToRemove.Contains(stateMachine))
                    continue;

                stateMachine.DoFixedUpdate();
            }

            CleanupCheck();
        }

        private void LateUpdate()
        {
            for (int i = 0; i < stateMachines.Count; i++)
            {
                IStateMachine stateMachine = stateMachines[i];

                if (stateMachinesToRemove.Contains(stateMachine))
                    continue;

                stateMachine.DoLateUpdate();
            }

            CleanupCheck();
        }

        private void CleanupCheck()
        {
            if (stateMachinesToRemove.Count > CleanupThreshold)
            {
                stateMachines.RemoveAll(fsm => stateMachinesToRemove.Contains(fsm));
                stateMachinesToRemove.Clear();
            }
        }
    }
}