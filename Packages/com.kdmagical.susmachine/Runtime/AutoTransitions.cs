using System;
using System.Collections;
using System.Collections.Generic;

namespace KDMagical.SUSMachine
{
    public enum TransitionType
    {
        Update,
        FixedUpdate,
        LateUpdate
    }

    /// <summary>
    /// A function that determines if a transition should be made.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>Null if no transition should be made, and state to transition to if transition should be made.</returns>
    public delegate T? AutoTransition<T>(IStateMachine<T> fsm) where T : struct, System.Enum;

    public interface ITransitions<TStates>
        where TStates : struct, System.Enum
    {
        void Initialize(IStateMachine<TStates> stateMachine);
        TStates? CheckTransitions(TransitionType transitionType);
        bool HasUpdateFunctions();
    }

    public class StatelessTransitions<TStates> : IEnumerable<AutoTransition<TStates>>, ITransitions<TStates>
        where TStates : struct, System.Enum
    {
        private Dictionary<TransitionType, List<AutoTransition<TStates>>> updateTransitions;

        protected IStateMachine<TStates> StateMachine { get; private set; }

        public void Initialize(IStateMachine<TStates> stateMachine)
        {
            this.StateMachine = stateMachine;
        }

        public void Add(AutoTransition<TStates> transition, TransitionType transitionType = TransitionType.Update) =>
            (updateTransitions ??= new Dictionary<TransitionType, List<AutoTransition<TStates>>>())
                .GetOrCreate(transitionType)
                .Add(transition);

        public void Add(Predicate<IStateMachine<TStates>> condition, TStates targetState, TransitionType transitionType = TransitionType.Update) =>
            (updateTransitions ??= new Dictionary<TransitionType, List<AutoTransition<TStates>>>())
                .GetOrCreate(transitionType)
                .Add(stateMachine =>
                    condition(stateMachine)
                        ? targetState
                        : (TStates?)null
                );

        /// <summary>
        /// Iterates through the transition checks and returns the first non-null result, or <see langword="null"/> if none.
        /// </summary>
        /// <param name="transitionType"></param>
        /// <returns>The first non-null state result, or <see langword="null"/> if none.</returns>
        public TStates? CheckTransitions(TransitionType transitionType)
        {
            if (updateTransitions == null || !updateTransitions.TryGetValue(transitionType, out var transitions))
                return null;

            foreach (var transition in transitions)
            {
                var result = transition(StateMachine);

                if (result != null)
                    return result;
            }
            return null;
        }

        public bool HasUpdateFunctions()
        {
            if (updateTransitions != null && updateTransitions.Count > 0)
            {
                foreach (var transitions in updateTransitions.Values)
                {
                    if (transitions != null && transitions.Count > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator() => throw new System.NotImplementedException();
        IEnumerator<AutoTransition<TStates>> IEnumerable<AutoTransition<TStates>>.GetEnumerator() => throw new System.NotImplementedException();
    }

    public class StatelessTransitions<TStates, TEvents> : StatelessTransitions<TStates>
        where TStates : struct, System.Enum
        where TEvents : struct, System.Enum
    {
        private Dictionary<TEvents, List<AutoTransition<TStates>>> eventTransitions;

        // complex case
        public void Add(AutoTransition<TStates> transition, TEvents fsmEvent) =>
            (eventTransitions ??= new Dictionary<TEvents, List<AutoTransition<TStates>>>())
                .GetOrCreate(fsmEvent)
                .Add(transition);

        // simple case
        public void Add(Predicate<IStateMachine<TStates>> condition, TStates targetState, TEvents fsmEvent) =>
            (eventTransitions ??= new Dictionary<TEvents, List<AutoTransition<TStates>>>())
                .GetOrCreate(fsmEvent)
                .Add(stateMachine =>
                    condition(stateMachine)
                        ? targetState
                        : (TStates?)null
                );

        // super simple (direct) case
        public void Add(TStates state, TEvents fsmEvent) =>
            (eventTransitions ??= new Dictionary<TEvents, List<AutoTransition<TStates>>>())
                .GetOrCreate(fsmEvent)
                .Add(_ => state);

        public TStates? CheckEventTransitions(TEvents fsmEvent)
        {
            if (eventTransitions == null || !eventTransitions.TryGetValue(fsmEvent, out var transitions))
                return null;

            foreach (var transition in transitions)
            {
                var result = transition(StateMachine);

                if (result != null)
                    return result;
            }
            return null;
        }
    }
}