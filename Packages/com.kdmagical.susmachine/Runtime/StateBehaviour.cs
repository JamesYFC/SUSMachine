﻿using System.Collections.Generic;

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

    public interface IStateObject<TStates, TEvents> : IStateObject<TStates>
        where TStates : struct, System.Enum
        where TEvents : struct, System.Enum
    {
        TStates? TriggerEventAndGetTransition(TEvents fsmEvent);
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

    public class Stateless<TStates, TEvents> : StatelessBase<TStates>, IStateObject<TStates, TEvents>
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

    public class StatefulContainer<TData> where TData : struct
    {
        public TData InitialData { get; set; }

        public TData CurrentData;

        public StatefulContainer() { }

        public StatefulContainer(TData initialData) => InitialData = initialData;

        public void Modify(TData newVal) => CurrentData = newVal;

        public void ResetData() => CurrentData = InitialData;
    }

    public delegate void StatefulAction<TStates, TData>(IStateMachine<TStates> stateMachine, TData currentData, System.Action<TData> modify)
        where TStates : struct, System.Enum
        where TData : struct;

    /// it seems we're forced to inherit from Stateless here and live with a bunch of shadowed members and duplicated code.
    /// This is because I want the api in StateMachine<T> where you don't have to type the constructor for Stateless<T> by default.
    /// Because indexers for a given parameter type can only be set for one type, we can't overload stateMachine[State] = new Stateful<T1, T2> without inheriting from Stateless<T>
    public class Stateful<TStates, TData> : Stateless<TStates>
        where TStates : struct, System.Enum
        where TData : struct
    {
        private StatefulTransitions<TStates, TData> transitions;
        public new StatefulTransitions<TStates, TData> Transitions =>
            transitions ??= new StatefulTransitions<TStates, TData>();

        protected override ITransitions<TStates> TransitionsBase => Transitions;

        private StatefulContainer<TData> data = new StatefulContainer<TData>();
        protected System.Action<TData> Modify => data.Modify;
        public TData CurrentData => data.CurrentData;
        public TData InitialData
        {
            set => data.InitialData = value;
        }

        public new StatefulAction<TStates, TData> OnEnter { get; set; }
        public new StatefulAction<TStates, TData> OnExit { get; set; }
        public new StatefulAction<TStates, TData> OnUpdate { get; set; }
        public new StatefulAction<TStates, TData> OnFixedUpdate { get; set; }
        public new StatefulAction<TStates, TData> OnLateUpdate { get; set; }

        public override void DoEnter()
            => OnEnter?.Invoke(StateMachine, CurrentData, Modify);
        public override void DoExit()
        {
            OnExit?.Invoke(StateMachine, CurrentData, Modify);
            data.ResetData();
        }
        public override void DoUpdate()
            => OnUpdate?.Invoke(StateMachine, CurrentData, Modify);
        public override void DoFixedUpdate()
            => OnFixedUpdate?.Invoke(StateMachine, CurrentData, Modify);
        public override void DoLateUpdate()
            => OnLateUpdate?.Invoke(StateMachine, CurrentData, Modify);

        public override void Initialize(IStateMachine<TStates> stateMachine)
        {
            data.ResetData();
            Transitions.SetDataContainer(data);
            base.Initialize(stateMachine);
        }

        public override bool HasUpdateFunctions()
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

    public class Stateful<TStates, TEvents, TData> : Stateless<TStates, TEvents>
        where TStates : struct, System.Enum
        where TEvents : struct, System.Enum
        where TData : struct
    {
        private StatefulTransitions<TStates, TEvents, TData> transitions;
        public new StatefulTransitions<TStates, TEvents, TData> Transitions =>
            transitions ??= new StatefulTransitions<TStates, TEvents, TData>();

        protected override ITransitions<TStates> TransitionsBase => Transitions;

        private StatefulContainer<TData> data = new StatefulContainer<TData>();

        protected System.Action<TData> Modify => data.Modify;
        public TData CurrentData => data.CurrentData;
        public TData InitialData
        {
            set => data.InitialData = value;
        }


        private Dictionary<TEvents, StatefulAction<TStates, TData>> onEvents;

        public new StatefulAction<TStates, TData> this[TEvents fsmEvent]
        {
            set =>
                (onEvents ??= new Dictionary<TEvents, StatefulAction<TStates, TData>>())
                    .Add(fsmEvent, value);
        }

        public new StatefulAction<TStates, TData> OnEnter { get; set; }
        public new StatefulAction<TStates, TData> OnExit { get; set; }
        public new StatefulAction<TStates, TData> OnUpdate { get; set; }
        public new StatefulAction<TStates, TData> OnFixedUpdate { get; set; }
        public new StatefulAction<TStates, TData> OnLateUpdate { get; set; }

        public override void DoEnter()
            => OnEnter?.Invoke(StateMachine, CurrentData, Modify);
        public override void DoExit()
        {
            OnExit?.Invoke(StateMachine, CurrentData, Modify);
            data.ResetData();
        }
        public override void DoUpdate()
            => OnUpdate?.Invoke(StateMachine, CurrentData, Modify);
        public override void DoFixedUpdate()
            => OnFixedUpdate?.Invoke(StateMachine, CurrentData, Modify);
        public override void DoLateUpdate()
            => OnLateUpdate?.Invoke(StateMachine, CurrentData, Modify);

        public override void Initialize(IStateMachine<TStates> stateMachine)
        {
            data.ResetData();
            Transitions.SetDataContainer(data);
            base.Initialize(stateMachine);
        }

        public override TStates? TriggerEventAndGetTransition(TEvents fsmEvent)
        {
            if (onEvents != null && onEvents.TryGetValue(fsmEvent, out var action))
                action(StateMachine, CurrentData, Modify);

            return Transitions.CheckEventTransitions(fsmEvent);
        }

        public override bool HasUpdateFunctions()
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
}