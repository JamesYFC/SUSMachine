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

    public class AutoTransitions<TStates> : IEnumerable<AutoTransition<TStates>>
        where TStates : struct, System.Enum
    {
        private Dictionary<TransitionType, List<AutoTransition<TStates>>> transitionsByType =
            new Dictionary<TransitionType, List<AutoTransition<TStates>>>();

        private IStateMachine<TStates> stateMachine;

        public void Initialize(IStateMachine<TStates> stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        public void Add(AutoTransition<TStates> transition, TransitionType transitionType = TransitionType.Update)
        {
            if (!transitionsByType.ContainsKey(transitionType))
                transitionsByType.Add(transitionType, new List<AutoTransition<TStates>>());

            transitionsByType[transitionType].Add(transition);
        }

        public void Add(Predicate<IStateMachine<TStates>> condition, TStates targetState, TransitionType transitionType = TransitionType.Update)
        {
            if (!transitionsByType.ContainsKey(transitionType))
                transitionsByType.Add(transitionType, new List<AutoTransition<TStates>>());

            transitionsByType[transitionType].Add(
                stateMachine => condition(stateMachine)
                    ? (TStates?)targetState
                    : null
                );
        }

        /// <summary>
        /// Iterates through the transition checks and returns the first non-null result, or <see langword="null"/> if none.
        /// </summary>
        /// <param name="fsm"></param>
        /// <param name="transitionType"></param>
        /// <returns>The first non-null state result, or <see langword="null"/> if none.</returns>
        public TStates? CheckTransitions(TransitionType transitionType)
        {
            if (!transitionsByType.ContainsKey(transitionType))
                return null;

            foreach (var transition in transitionsByType[transitionType])
            {
                var result = transition(stateMachine);

                if (result != null)
                    return result;
            }
            return null;
        }

        IEnumerator IEnumerable.GetEnumerator() => throw new System.NotImplementedException();
        IEnumerator<AutoTransition<TStates>> IEnumerable<AutoTransition<TStates>>.GetEnumerator() => throw new System.NotImplementedException();
    }
}