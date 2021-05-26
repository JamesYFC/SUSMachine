namespace KDMagical.SUSMachine
{
    public delegate void StateAction<T>(IStateMachine<T> state) where T : struct, System.Enum;

    /// <summary>
    /// Contains the Action delegates and AutoTransitions of a particular state.
    /// </summary>
    /// <typeparam name="TStates"></typeparam>
    public class StateBehaviour<TStates>
        where TStates : struct, System.Enum
    {
        private IStateMachine<TStates> stateMachine;

        public AutoTransitions<TStates> AutoTransitions { get; set; } = new AutoTransitions<TStates>();

        public StateAction<TStates> OnEnter { get; set; }
        public StateAction<TStates> OnExit { get; set; }
        public StateAction<TStates> OnUpdate { get; set; }
        public StateAction<TStates> OnFixedUpdate { get; set; }
        public StateAction<TStates> OnLateUpdate { get; set; }

        public virtual void Initialize(IStateMachine<TStates> stateMachine)
        {
            this.stateMachine = stateMachine;
            AutoTransitions?.Initialize(stateMachine);
        }

        public virtual void DoEnter()
            => OnEnter?.Invoke(stateMachine);
        public virtual void DoExit()
            => OnExit?.Invoke(stateMachine);
        public virtual void DoUpdate()
            => OnUpdate?.Invoke(stateMachine);
        public virtual void DoFixedUpdate()
            => OnFixedUpdate?.Invoke(stateMachine);
        public virtual void DoLateUpdate()
            => OnLateUpdate?.Invoke(stateMachine);

        /// <inheritdoc cref="KDMagical.SUSMachine.AutoTransitions{TStates}.CheckTransitions(IStateMachine{TStates}, TransitionType)" />
        public virtual TStates? CheckAutoTransitions(TransitionType transitionType)
            => AutoTransitions?.CheckTransitions(transitionType);
    }
}