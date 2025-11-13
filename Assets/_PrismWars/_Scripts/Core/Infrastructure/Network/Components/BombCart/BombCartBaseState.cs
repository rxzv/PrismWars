namespace _PrismWars._Scripts.Core.Infrastructure.Network.Components.BombCart {
    public abstract class BombCartBaseState {
        public abstract BombCartState StateType { get; }
        protected BombCartStateMachine StateMachine { get; }

        protected BombCartBaseState(BombCartStateMachine stateMachine) =>
            StateMachine = stateMachine;

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void Exit() { }
    }
}