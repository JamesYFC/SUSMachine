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

    public interface ITransitions<TStates>
        where TStates : struct, System.Enum
    {
        void Initialize(IStateMachine<TStates> stateMachine);
        TStates? CheckTransitions(TransitionType transitionType);
        bool HasUpdateFunctions();
    }

    public abstract class TransitionsBase<TStates, TTransition> : ITransitions<TStates>, IEnumerable<TTransition>
        where TStates : struct, System.Enum
        where TTransition : System.Delegate
    {
        protected Dictionary<TransitionType, List<TTransition>> updateTransitions;
        protected Dictionary<TransitionType, List<TTransition>> UpdateTransitions => updateTransitions ??= new Dictionary<TransitionType, List<TTransition>>();

        protected IStateMachine<TStates> StateMachine { get; private set; }

        public virtual void Initialize(IStateMachine<TStates> stateMachine)
        {
            this.StateMachine = stateMachine;
        }

        public void Add(TTransition transition, TransitionType transitionType = TransitionType.Update) =>
            (updateTransitions ??= new Dictionary<TransitionType, List<TTransition>>())
                .GetOrCreate(transitionType)
                .Add(transition);

        /// <summary>
        /// Iterates through the transition checks and returns the first non-null result, or <see langword="null"/> if none.
        /// </summary>
        /// <param name="transitionType"></param>
        /// <returns>The first non-null state result, or <see langword="null"/> if none.</returns>
        public abstract TStates? CheckTransitions(TransitionType transitionType);

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
        IEnumerator<TTransition> IEnumerable<TTransition>.GetEnumerator() => throw new System.NotImplementedException();
    }
}