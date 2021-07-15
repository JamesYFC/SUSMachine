using System.Collections.Generic;

namespace KDMagical.SUSMachine
{
    public interface IStateObject<T> where T : struct, System.Enum
    {
        void Initialize(IStateMachine<T> stateMachine);

        void DoEnter();
        void DoExit();
        void DoUpdate();
        void DoFixedUpdate();
        void DoLateUpdate();

        T? CheckAutoTransitions(TransitionType transitionType);
        bool HasUpdateFunctions();
    }

    public delegate void StateAction<T>(IStateMachine<T> stateMachine) where T : struct, System.Enum;

    public abstract class StateObjectBase<TStates, TCallback> : IStateObject<TStates>
        where TStates : struct, System.Enum
    {
        protected IStateMachine<TStates> StateMachine { get; set; }
        protected abstract ITransitions<TStates> TransitionsBase { get; }

        public virtual void Initialize(IStateMachine<TStates> stateMachine)
        {
            this.StateMachine = stateMachine;
            TransitionsBase?.Initialize(stateMachine);
        }

        public TCallback OnEnter { get; set; }
        public TCallback OnExit { get; set; }
        public TCallback OnUpdate { get; set; }
        public TCallback OnFixedUpdate { get; set; }
        public TCallback OnLateUpdate { get; set; }

        public abstract void DoEnter();
        public abstract void DoExit();
        public abstract void DoUpdate();
        public abstract void DoFixedUpdate();
        public abstract void DoLateUpdate();

        /// <inheritdoc cref="KDMagical.SUSMachine.StatelessTransitions{TStates}.CheckTransitions(IStateMachine{TStates}, TransitionType)" />
        public virtual TStates? CheckAutoTransitions(TransitionType transitionType)
            => TransitionsBase.CheckTransitions(transitionType);

        public virtual bool HasUpdateFunctions()
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

    public abstract class StatelessBase<TStates> : StateObjectBase<TStates, StateAction<TStates>>
        where TStates : struct, System.Enum
    {

        public override void DoEnter()
            => OnEnter?.Invoke(StateMachine);
        public override void DoExit()
            => OnExit?.Invoke(StateMachine);
        public override void DoUpdate()
            => OnUpdate?.Invoke(StateMachine);
        public override void DoFixedUpdate()
            => OnFixedUpdate?.Invoke(StateMachine);
        public override void DoLateUpdate()
            => OnLateUpdate?.Invoke(StateMachine);
    }

    /// <summary>
    /// Contains the Action delegates and AutoTransitions of a particular state.
    /// </summary>
    /// <typeparam name="TStates"></typeparam>
    public class Stateless<TStates> : StatelessBase<TStates>
        where TStates : struct, System.Enum
    {
        private StatelessTransitions<TStates> transitions;
        public StatelessTransitions<TStates> Transitions =>
            transitions ??= new StatelessTransitions<TStates>();

        protected override ITransitions<TStates> TransitionsBase => Transitions;
    }

    public class Stateless<TStates, TEvents> : StatelessBase<TStates>
        where TStates : struct, System.Enum
        where TEvents : struct, System.Enum
    {
        private StatelessTransitions<TStates, TEvents> transitions;
        public StatelessTransitions<TStates, TEvents> Transitions =>
            transitions ??= new StatelessTransitions<TStates, TEvents>();
        protected override ITransitions<TStates> TransitionsBase => Transitions;

        private Dictionary<TEvents, StateAction<TStates>> onEvents;

        public StateAction<TStates> this[TEvents fsmEvent]
        {
            set =>
                (onEvents ??= new Dictionary<TEvents, StateAction<TStates>>())
                    .Add(fsmEvent, value);
        }

        public virtual TStates? TriggerEventAndGetTransition(TEvents fsmEvent)
        {
            if (onEvents != null && onEvents.TryGetValue(fsmEvent, out var action))
                action(StateMachine);

            return Transitions.CheckEventTransitions(fsmEvent);
        }
    }

    public delegate void DataStateAction<TStates, TData>(IStateMachine<TStates> stateMachine, TData currentData)
        where TStates : struct, System.Enum
        where TData : struct;

    public abstract class StatefulBase<TStates, TData> : StateObjectBase<TStates, DataStateAction<TStates, TData>>
        where TStates : struct, System.Enum
        where TData : struct
    {
        public TData InitialData { get; set; }

        protected TData currentData;
        public ref TData CurrentData => ref currentData;

        public override void Initialize(IStateMachine<TStates> stateMachine)
        {
            base.Initialize(stateMachine);
            ResetData();
        }

        protected void ResetData()
        {
            currentData = InitialData;
        }
    }

    public class Stateful<TStates, TData> : StatefulBase<TStates, TData>
        where TStates : struct, System.Enum
        where TData : struct
    {
        private StatelessTransitions<TStates> transitions;
        public StatelessTransitions<TStates> Transitions =>
            transitions ??= new StatelessTransitions<TStates>();

        protected override ITransitions<TStates> TransitionsBase => throw new System.NotImplementedException();

        public override void DoEnter()
            => OnEnter?.Invoke(StateMachine, CurrentData);
        public override void DoExit()
            => OnExit?.Invoke(StateMachine, CurrentData);
        public override void DoUpdate()
            => OnUpdate?.Invoke(StateMachine, CurrentData);
        public override void DoFixedUpdate()
            => OnFixedUpdate?.Invoke(StateMachine, CurrentData);
        public override void DoLateUpdate()
            => OnLateUpdate?.Invoke(StateMachine, CurrentData);
    }
}