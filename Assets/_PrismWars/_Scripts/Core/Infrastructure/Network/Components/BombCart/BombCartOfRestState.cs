using _PrismWars._Scripts.Game.Player.Model;

namespace _PrismWars._Scripts.Core.Infrastructure.Network.Components.BombCart {
    public class BombCartOfRestState : BombCartBaseState {
        public override BombCartState StateType => BombCartState.OfRest;
    
        BombCartMovement _movement;
        BombCartTriggerHandler _triggerHandler;

        public BombCartOfRestState(BombCartStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter() {
            _movement = StateMachine.GetComponent<BombCartMovement>();
            _triggerHandler = StateMachine.GetComponent<BombCartTriggerHandler>();
        }

        public override void Update() {
            var dominantElement = _triggerHandler.GetDominantElement();
            if (dominantElement != PlayerElement.None) {
                _movement.SetTargetElement(dominantElement);
                StateMachine.SetState(StateMachine.OfMotionState);
            }
        }
    }
}