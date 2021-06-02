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

        void Initialize(T initialState);
        void Close();

        void SetState(T newState);
    }

    public abstract class StateMachineBase<T> : IStateMachine<T> where T : struct, System.Enum
    {
        public T CurrentState { get; protected set; }

        protected abstract IEnumerable<IStateBehaviour<T>> StateBehaviours { get; }
        protected abstract IStateBehaviour<T> CurrentStateBehaviourBase { get; }

        protected float currentStateEnterTime;
        public float TimeInState => Time.time - currentStateEnterTime;

        protected IStateMachineManager stateMachineManager { get; private set; }

        protected bool HasUpdateFunctions { get; private set; }

        public StateMachineBase() : this(StateMachineManager.Instance) { }

        public StateMachineBase(IStateMachineManager stateMachineManager)
        {
            this.stateMachineManager = stateMachineManager;
        }

        public void Initialize(T initialState)
        {
            foreach (var behaviour in StateBehaviours)
            {
                behaviour.Initialize(this);
            }

            HasUpdateFunctions = CheckForUpdateFunctions();
            if (HasUpdateFunctions)
                stateMachineManager.Register(this);

            CurrentState = initialState;
            currentStateEnterTime = Time.time;
            CurrentStateBehaviourBase.DoEnter();
        }

        /// <summary>
        /// Returns false if no update loop actions or transitions are set on any behaviours in this state machine.
        /// </summary>
        /// <returns></returns>
        protected virtual bool CheckForUpdateFunctions()
        {
            foreach (var behaviour in StateBehaviours)
            {
                if (behaviour.HasUpdateFunctions())
                    return true;
            }
            return false;
        }

        public void Close()
        {
            if (HasUpdateFunctions)
                stateMachineManager.Deregister(this);
        }

        public void DoUpdate()
        {
            CurrentStateBehaviourBase.DoUpdate();
            CheckForAutoTransitions(TransitionType.Update);
        }

        public void DoFixedUpdate()
        {
            CurrentStateBehaviourBase.DoFixedUpdate();
            CheckForAutoTransitions(TransitionType.FixedUpdate);
        }

        public void DoLateUpdate()
        {
            CurrentStateBehaviourBase.DoLateUpdate();
            CheckForAutoTransitions(TransitionType.LateUpdate);
        }

        private void CheckForAutoTransitions(TransitionType transitionMode)
        {
            var transitionResult = CurrentStateBehaviourBase.CheckAutoTransitions(transitionMode);
            if (transitionResult != null)
                SetState(transitionResult.Value);
        }

        public void SetState(T newState)
        {
            CurrentStateBehaviourBase.DoExit();
            CurrentState = newState;
            currentStateEnterTime = Time.time;
            CurrentStateBehaviourBase.DoEnter();
        }
    }

    public class StateMachine<T> : StateMachineBase<T> where T : struct, System.Enum
    {
        private Dictionary<T, StateBehaviour<T>> stateBehaviours = new Dictionary<T, StateBehaviour<T>>();
        protected override IEnumerable<IStateBehaviour<T>> StateBehaviours => stateBehaviours.Values;

        public StateBehaviour<T> this[T state]
        {
            get => stateBehaviours.GetOrCreate(state);
            set => stateBehaviours[state] = value;
        }

        public StateBehaviour<T> CurrentStateBehaviour => this[CurrentState];
        protected override IStateBehaviour<T> CurrentStateBehaviourBase => CurrentStateBehaviour;

        public StateMachine() : base() { }

        public StateMachine(IStateMachineManager stateMachineManager) : base(stateMachineManager) { }
    }

    public class StateMachine<TStates, TEvents> : StateMachineBase<TStates>
        where TStates : struct, System.Enum
        where TEvents : struct, System.Enum
    {
        private Dictionary<TStates, StateBehaviour<TStates, TEvents>> stateBehaviours =
            new Dictionary<TStates, StateBehaviour<TStates, TEvents>>();

        protected override IEnumerable<IStateBehaviour<TStates>> StateBehaviours => stateBehaviours.Values;

        public StateBehaviour<TStates, TEvents> this[TStates state]
        {
            get => stateBehaviours.GetOrCreate(state);
            set => stateBehaviours[state] = value;
        }

        public StateBehaviour<TStates, TEvents> CurrentStateBehaviour => this[CurrentState];
        protected override IStateBehaviour<TStates> CurrentStateBehaviourBase => CurrentStateBehaviour;

        public StateMachine() : base() { }

        public StateMachine(IStateMachineManager stateMachineManager) : base(stateMachineManager) { }

        public void TriggerEvent(TEvents fsmEvent)
        {
            var nextState = CurrentStateBehaviour.TriggerEvent(fsmEvent);
            if (nextState != null)
                SetState(nextState.Value);
        }
    }
}