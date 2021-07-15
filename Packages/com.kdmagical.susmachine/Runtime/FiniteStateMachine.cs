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

        void Close();
    }

    public interface IStateMachine<T> : IStateMachine where T : struct, System.Enum
    {
        T CurrentState { get; }
        void Initialize(T initialState, GameObject closeOnDestroy = null);
        void SetState(T newState);
    }

    public interface IEventTriggerable<T> where T : struct, System.Enum
    {
        void TriggerEvent(T triggeredEvent);
    }

    public abstract class StateMachineBase<T> : IStateMachine<T> where T : struct, System.Enum
    {
        public T CurrentState { get; protected set; }

        protected abstract IEnumerable<IStateObject<T>> StateBehaviours { get; }
        protected abstract IStateObject<T> CurrentStateBehaviourBase { get; }
        protected abstract IStateObject<T> AnyStateBase { get; }

        protected float currentStateEnterTime;
        public float TimeInState => Time.time - currentStateEnterTime;

        protected IStateMachineManager stateMachineManager { get; private set; }

        protected bool HasUpdateFunctions { get; private set; }

        public StateMachineBase() : this(StateMachineManager.Instance) { }

        public StateMachineBase(IStateMachineManager stateMachineManager)
        {
            this.stateMachineManager = stateMachineManager;
        }

        /// <summary>
        /// Initializes this state machine and Close() automatically when <paramref name="closeOnDestroy"/> is destroyed.
        /// </summary>
        /// <param name="initialState"></param>
        /// <param name="closeOnDestroy"></param>
        public void Initialize(T initialState, GameObject closeOnDestroy = null)
        {
            HasUpdateFunctions = CheckForUpdateFunctions();
            if (HasUpdateFunctions)
            {
                stateMachineManager.Register(this);

                if (closeOnDestroy != null)
                {
                    var autoCloser = closeOnDestroy.AddComponent<StateMachineAutoCloser>();
                    autoCloser.Initialize(this);
                }
            }

            AnyStateBase?.Initialize(this);
            foreach (var behaviour in StateBehaviours)
            {
                behaviour.Initialize(this);
            }

            CurrentState = initialState;
            currentStateEnterTime = Time.time;
            DoEnter();
        }

        /// <summary>
        /// Returns false if no update loop actions or transitions are set on any behaviours in this state machine.
        /// </summary>
        /// <returns></returns>
        protected virtual bool CheckForUpdateFunctions()
        {
            if (AnyStateBase != null && AnyStateBase.HasUpdateFunctions())
                return true;

            foreach (var behaviour in StateBehaviours)
            {
                if (behaviour.HasUpdateFunctions())
                    return true;
            }
            return false;
        }

        public void Close()
        {
            DoExit();
            if (HasUpdateFunctions)
                stateMachineManager.Deregister(this);
        }

        protected void DoEnter()
        {
            AnyStateBase?.DoEnter();
            CurrentStateBehaviourBase.DoEnter();
        }

        protected void DoExit()
        {
            AnyStateBase?.DoExit();
            CurrentStateBehaviourBase.DoExit();
        }

        public void DoUpdate()
        {
            AnyStateBase?.DoUpdate();
            CurrentStateBehaviourBase.DoUpdate();
            CheckForAutoTransitions(TransitionType.Update);
        }

        public void DoFixedUpdate()
        {
            AnyStateBase?.DoFixedUpdate();
            CurrentStateBehaviourBase.DoFixedUpdate();
            CheckForAutoTransitions(TransitionType.FixedUpdate);
        }

        public void DoLateUpdate()
        {
            AnyStateBase?.DoLateUpdate();
            CurrentStateBehaviourBase.DoLateUpdate();
            CheckForAutoTransitions(TransitionType.LateUpdate);
        }

        private void CheckForAutoTransitions(TransitionType transitionMode)
        {
            var transitionResult = CurrentStateBehaviourBase.CheckAutoTransitions(transitionMode);
            if (transitionResult != null)
            {
                SetState(transitionResult.Value);
                return;
            }

            transitionResult = AnyStateBase?.CheckAutoTransitions(transitionMode);
            if (transitionResult != null)
                SetState(transitionResult.Value);
        }

        /// <summary>
        /// <para>Sets the current state of the state machine to <paramref name="newState"/>.</para>
        /// 
        /// <para>Avoid using this method in any StateBehaviour action, and <b>**DO NOT call this in a StateBehaviour's OnEnter() or OnExit()!**</b></para>
        /// </summary>
        /// <remarks>
        /// It is recommended to use the built-in events and transitions support over this function for most use cases.
        /// </remarks>
        /// <param name="newState"></param>
        public void SetState(T newState)
        {
            DoExit();
            CurrentState = newState;
            currentStateEnterTime = Time.time;
            DoEnter();
        }
    }

    public class StateMachine<T> : StateMachineBase<T> where T : struct, System.Enum
    {
        private Dictionary<T, Stateless<T>> stateBehaviours = new Dictionary<T, Stateless<T>>();
        protected override IEnumerable<IStateObject<T>> StateBehaviours => stateBehaviours.Values;

        public Stateless<T> this[T state]
        {
            get => stateBehaviours.GetOrCreate(state);
            set => stateBehaviours[state] = value;
        }

        public Stateless<T> CurrentStateBehaviour => this[CurrentState];
        protected override IStateObject<T> CurrentStateBehaviourBase => CurrentStateBehaviour;

        private Stateless<T> anyState;
        protected override IStateObject<T> AnyStateBase => anyState;

        public Stateless<T> AnyState
        {
            get => anyState ??= new Stateless<T>();
            set => anyState = value;
        }

        public StateMachine() : base() { }

        public StateMachine(IStateMachineManager stateMachineManager) : base(stateMachineManager) { }
    }

    public class StateMachine<TStates, TEvents> : StateMachineBase<TStates>, IEventTriggerable<TEvents>
        where TStates : struct, System.Enum
        where TEvents : struct, System.Enum
    {
        private Dictionary<TStates, Stateless<TStates, TEvents>> stateBehaviours =
            new Dictionary<TStates, Stateless<TStates, TEvents>>();

        protected override IEnumerable<IStateObject<TStates>> StateBehaviours => stateBehaviours.Values;

        public Stateless<TStates, TEvents> this[TStates state]
        {
            get => stateBehaviours.GetOrCreate(state);
            set => stateBehaviours[state] = value;
        }

        public Stateless<TStates, TEvents> CurrentStateBehaviour => this[CurrentState];
        protected override IStateObject<TStates> CurrentStateBehaviourBase => CurrentStateBehaviour;

        private Stateless<TStates, TEvents> anyState;
        protected override IStateObject<TStates> AnyStateBase => anyState;

        public Stateless<TStates, TEvents> AnyState
        {
            get => anyState ??= new Stateless<TStates, TEvents>();
            set => anyState = value;
        }

        public StateMachine() : base() { }

        public StateMachine(IStateMachineManager stateMachineManager) : base(stateMachineManager) { }

        public void TriggerEvent(TEvents fsmEvent)
        {
            var nextState = CurrentStateBehaviour.TriggerEventAndGetTransition(fsmEvent);
            if (nextState != null)
            {
                SetState(nextState.Value);
                return;
            }

            nextState = anyState?.TriggerEventAndGetTransition(fsmEvent);
            if (nextState != null)
                SetState(nextState.Value);
        }
    }
}