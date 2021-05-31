using System.Collections.Generic;

namespace KDMagical.SUSMachine
{
    public interface IStateBehaviour<T> where T : struct, System.Enum
    {
        void Initialize(IStateMachine<T> stateMachine);

        StateAction<T> OnEnter { get; }
        StateAction<T> OnExit { get; }
        StateAction<T> OnUpdate { get; }
        StateAction<T> OnFixedUpdate { get; }
        StateAction<T> OnLateUpdate { get; }

        void DoEnter();
        void DoExit();
        void DoUpdate();
        void DoFixedUpdate();
        void DoLateUpdate();

        T? CheckAutoTransitions(TransitionType transitionType);
        bool HasUpdateFunctions();
    }

    public delegate void StateAction<T>(IStateMachine<T> stateMachine) where T : struct, System.Enum;

    public abstract class StateBehaviourBase<TStates> : IStateBehaviour<TStates>
        where TStates : struct, System.Enum
    {
        protected IStateMachine<TStates> StateMachine { get; private set; }

        protected abstract Transitions<TStates> TransitionsBase { get; }

        public StateAction<TStates> OnEnter { get; set; }
        public StateAction<TStates> OnExit { get; set; }
        public StateAction<TStates> OnUpdate { get; set; }
        public StateAction<TStates> OnFixedUpdate { get; set; }
        public StateAction<TStates> OnLateUpdate { get; set; }

        public virtual void Initialize(IStateMachine<TStates> stateMachine)
        {
            this.StateMachine = stateMachine;
            TransitionsBase?.Initialize(stateMachine);
        }

        public virtual void DoEnter()
            => OnEnter?.Invoke(StateMachine);
        public virtual void DoExit()
            => OnExit?.Invoke(StateMachine);
        public virtual void DoUpdate()
            => OnUpdate?.Invoke(StateMachine);
        public virtual void DoFixedUpdate()
            => OnFixedUpdate?.Invoke(StateMachine);
        public virtual void DoLateUpdate()
            => OnLateUpdate?.Invoke(StateMachine);

        /// <inheritdoc cref="KDMagical.SUSMachine.Transitions{TStates}.CheckTransitions(IStateMachine{TStates}, TransitionType)" />
        public virtual TStates? CheckAutoTransitions(TransitionType transitionType)
            => TransitionsBase?.CheckTransitions(transitionType);

        public bool HasUpdateFunctions()
        {
            if ((
                    OnUpdate ??
                    OnFixedUpdate ??
                    OnLateUpdate
                ) != null)
            {
                return true;
            }

            if (TransitionsBase != null && TransitionsBase.HasUpdateFunctions())
                return true;

            return false;
        }
    }

    /// <summary>
    /// Contains the Action delegates and AutoTransitions of a particular state.
    /// </summary>
    /// <typeparam name="TStates"></typeparam>
    public class StateBehaviour<TStates> : StateBehaviourBase<TStates>
        where TStates : struct, System.Enum
    {
        private Transitions<TStates> transitions;
        public Transitions<TStates> Transitions =>
            transitions ??= new Transitions<TStates>();

        protected override Transitions<TStates> TransitionsBase => Transitions;
    }

    public class StateBehaviour<TStates, TEvents> : StateBehaviourBase<TStates>
        where TStates : struct, System.Enum
        where TEvents : struct, System.Enum
    {
        private Transitions<TStates, TEvents> transitions;
        public Transitions<TStates, TEvents> Transitions =>
            transitions ??= new Transitions<TStates, TEvents>();

        protected override Transitions<TStates> TransitionsBase => Transitions;

        private Dictionary<TEvents, StateAction<TStates>> onEvents;
        public Dictionary<TEvents, StateAction<TStates>> OnEvents =>
            onEvents ??= new Dictionary<TEvents, StateAction<TStates>>();

        public TStates? TriggerEvent(TEvents fsmEvent)
        {
            if (OnEvents != null && OnEvents.TryGetValue(fsmEvent, out var action))
                action(StateMachine);

            return Transitions.CheckEventTransitions(fsmEvent);
        }
    }
}