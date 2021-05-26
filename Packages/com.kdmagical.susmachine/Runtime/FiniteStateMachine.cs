using System.Collections.Generic;
using UnityEngine;

namespace KDMagical.SUSMachine
{
    public interface IStateMachine
    {
        float TimeInState { get; }

        void DoUpdate();
        void DoFixedUpdate();
        void DoLateUpdate();
    }

    public interface IStateMachine<T> : IStateMachine where T : struct, System.Enum
    {
        T CurrentState { get; }
        StateBehaviour<T> this[T state] { get; }

        void Initialize(T initialState);
        void Close();

        void SetState(T newState);
    }

    public class StateMachine<T> : IStateMachine<T> where T : struct, System.Enum
    {
        event System.EventHandler someEvent;
        UnityEngine.Events.UnityEvent someUnityEvent;

        private Dictionary<T, StateBehaviour<T>> stateBehaviours = new Dictionary<T, StateBehaviour<T>>();

        public StateBehaviour<T> this[T state]
        {
            get
            {
                if (!stateBehaviours.ContainsKey(state))
                {
                    stateBehaviours[state] = new StateBehaviour<T>();
                }
                return stateBehaviours[state];
            }
            set => stateBehaviours[state] = value;
        }

        public StateBehaviour<T> CurrentStateBehaviour => this[CurrentState];

        public T CurrentState { get; private set; }

        private float currentStateEnterTime;
        public float TimeInState => Time.time - currentStateEnterTime;

        private IStateMachineManager stateMachineManager;

        public StateMachine() : this(StateMachineManager.Instance) { }

        public StateMachine(IStateMachineManager stateMachineManager)
        {
            this.stateMachineManager = stateMachineManager;
        }

        public void Initialize(T initialState)
        {
            foreach (var behaviour in stateBehaviours.Values)
            {
                behaviour.Initialize(this);
            }

            stateMachineManager.Register(this);
            CurrentState = initialState;
            currentStateEnterTime = Time.time;
            CurrentStateBehaviour.DoEnter();
        }

        public void Close()
        {
            stateMachineManager.Deregister(this);
        }

        public void SetState(T newState)
        {
            CurrentStateBehaviour.DoExit();
            CurrentState = newState;
            currentStateEnterTime = Time.time;
            CurrentStateBehaviour.DoEnter();
        }

        public void DoUpdate()
        {
            CheckForAutoTransitions(TransitionType.Update);
            CurrentStateBehaviour.DoUpdate();
        }

        public void DoFixedUpdate()
        {
            CheckForAutoTransitions(TransitionType.FixedUpdate);
            CurrentStateBehaviour.DoFixedUpdate();
        }

        public void DoLateUpdate()
        {
            CheckForAutoTransitions(TransitionType.LateUpdate);
            CurrentStateBehaviour.DoLateUpdate();
        }

        private void CheckForAutoTransitions(TransitionType transitionMode)
        {
            var transitionResult = CurrentStateBehaviour.CheckAutoTransitions(transitionMode);
            if (transitionResult != null)
                SetState(transitionResult.Value);
        }
    }
}