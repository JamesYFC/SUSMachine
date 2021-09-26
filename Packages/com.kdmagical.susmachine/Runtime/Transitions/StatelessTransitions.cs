using System;
using System.Collections;
using System.Collections.Generic;

namespace KDMagical.SUSMachine
{
    /// <summary>
    /// A function that determines if a transition should be made.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>Null if no transition should be made, and state to transition to if transition should be made.</returns>
    public delegate T? Transition<T>(IStateMachine<T> fsm) where T : struct, System.Enum;

    public class StatelessTransitions<TStates> : TransitionsBase<TStates, Transition<TStates>>
        where TStates : struct, System.Enum
    {
        public void Add(Predicate<IStateMachine<TStates>> condition, TStates targetState, TransitionType transitionType = TransitionType.Update) =>
            UpdateTransitions
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
        public override TStates? CheckTransitions(TransitionType transitionType)
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
    }

    public class StatelessTransitions<TStates, TEvents> : StatelessTransitions<TStates>
        where TStates : struct, System.Enum
        where TEvents : struct, System.Enum
    {
        private Dictionary<TEvents, List<Transition<TStates>>> eventTransitions;
        private Dictionary<TEvents, List<Transition<TStates>>> EventTransitions
            => eventTransitions ??= new Dictionary<TEvents, List<Transition<TStates>>>();

        // complex case
        public void Add(TEvents fsmEvent, Transition<TStates> transition) =>
            EventTransitions
                .GetOrCreate(fsmEvent)
                .Add(transition);

        // simple case
        public void Add(TEvents fsmEvent, Predicate<IStateMachine<TStates>> condition, TStates targetState) =>
            EventTransitions
                .GetOrCreate(fsmEvent)
                .Add(stateMachine =>
                    condition(stateMachine)
                        ? targetState
                        : (TStates?)null
                );

        // super simple (direct) case
        public void Add(TEvents fsmEvent, TStates state) =>
            EventTransitions
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