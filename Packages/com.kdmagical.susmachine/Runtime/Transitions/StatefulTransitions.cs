using System;
using System.Collections.Generic;

namespace KDMagical.SUSMachine
{
    public delegate TStates? StatefulTransition<TStates, TData>(IStateMachine<TStates> fsm, TData stateData)
        where TStates : struct, System.Enum
        where TData : struct;

    public class StatefulTransitions<TStates, TData> : TransitionsBase<TStates, StatefulTransition<TStates, TData>>
        where TStates : struct, System.Enum
        where TData : struct
    {
        protected StatefulContainer<TData> StateDataContainer { get; private set; }

        public void SetDataContainer(StatefulContainer<TData> dataContainer)
        {
            StateDataContainer = dataContainer;
        }

        public void Add(Transition<TStates> transition, TransitionType transitionType = TransitionType.Update) =>
            UpdateTransitions
                .GetOrCreate(transitionType)
                .Add(transition.Convert<TStates, TData>());

        public void Add(Predicate<IStateMachine<TStates>, TData> condition, TStates targetState, TransitionType transitionType = TransitionType.Update) =>
            UpdateTransitions
                .GetOrCreate(transitionType)
                .Add((stateMachine, stateData) =>
                    condition(stateMachine, stateData)
                        ? targetState
                        : (TStates?)null
                );

        public void Add(Predicate<IStateMachine<TStates>> condition, TStates targetState, TransitionType transitionType = TransitionType.Update) =>
            UpdateTransitions
                .GetOrCreate(transitionType)
                .Add((stateMachine, _) =>
                    condition(stateMachine)
                        ? targetState
                        : (TStates?)null
                );

        public override TStates? CheckTransitions(TransitionType transitionType)
        {
            if (updateTransitions == null || !updateTransitions.TryGetValue(transitionType, out var transitions))
                return null;

            foreach (var transition in transitions)
            {
                var result = transition(StateMachine, StateDataContainer.CurrentData);

                if (result != null)
                    return result;
            }
            return null;
        }
    }

    public class StatefulTransitions<TStates, TEvents, TData> : StatefulTransitions<TStates, TData>
        where TStates : struct, System.Enum
        where TEvents : struct, System.Enum
        where TData : struct
    {
        private Dictionary<TEvents, List<StatefulTransition<TStates, TData>>> eventTransitions;
        protected Dictionary<TEvents, List<StatefulTransition<TStates, TData>>> EventTransitions =>
            eventTransitions ??= new Dictionary<TEvents, List<StatefulTransition<TStates, TData>>>();

        // complex case
        public void Add(StatefulTransition<TStates, TData> transition, TEvents fsmEvent) =>
            (eventTransitions ??= new Dictionary<TEvents, List<StatefulTransition<TStates, TData>>>())
                .GetOrCreate(fsmEvent)
                .Add(transition);

        public void Add(Transition<TStates> transition, TEvents fsmEvent) =>
        (eventTransitions ??= new Dictionary<TEvents, List<StatefulTransition<TStates, TData>>>())
            .GetOrCreate(fsmEvent)
            .Add(transition.Convert<TStates, TData>());

        // simple case
        public void Add(Predicate<IStateMachine<TStates>, TData> condition, TStates targetState, TEvents fsmEvent) =>
            (eventTransitions ??= new Dictionary<TEvents, List<StatefulTransition<TStates, TData>>>())
                .GetOrCreate(fsmEvent)
                .Add((stateMachine, stateData) =>
                    condition(stateMachine, stateData)
                        ? targetState
                        : (TStates?)null
                );

        public void Add(Predicate<IStateMachine<TStates>> condition, TStates targetState, TEvents fsmEvent) =>
            (eventTransitions ??= new Dictionary<TEvents, List<StatefulTransition<TStates, TData>>>())
                .GetOrCreate(fsmEvent)
                .Add((stateMachine, _) =>
                    condition(stateMachine)
                        ? targetState
                        : (TStates?)null
                );

        // super simple (direct) case
        public void Add(TStates state, TEvents fsmEvent) =>
            (eventTransitions ??= new Dictionary<TEvents, List<StatefulTransition<TStates, TData>>>())
                .GetOrCreate(fsmEvent)
                .Add((_, __) => state);

        public TStates? CheckEventTransitions(TEvents fsmEvent)
        {
            if (eventTransitions == null || !eventTransitions.TryGetValue(fsmEvent, out var transitions))
                return null;

            foreach (var transition in transitions)
            {
                var result = transition(StateMachine, StateDataContainer.CurrentData);

                if (result != null)
                    return result;
            }
            return null;
        }
    }

    internal static class TransitionExtensions
    {
        public static StatefulTransition<TStates, TData> Convert<TStates, TData>(this Transition<TStates> transition)
            where TStates : struct, System.Enum
            where TData : struct
                => (IStateMachine<TStates> fsm, TData stateData) => transition(fsm);
    }
}