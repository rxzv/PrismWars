using _PrismWars._Scripts.Game.Player.Model;

namespace _PrismWars._Scripts.Core.Infrastructure.Network.Components.BombCart {
    public class BombCartOfMotionState : BombCartBaseState {
        public override BombCartState StateType => BombCartState.OfMotion;
    
        BombCartMovement _movement;
        BombCartTriggerHandler _triggerHandler;

        public BombCartOfMotionState(BombCartStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter() {
            _movement = StateMachine.GetComponent<BombCartMovement>();
            _triggerHandler = StateMachine.GetComponent<BombCartTriggerHandler>();
        }

        public override void Update() {
            var dominantElement = _triggerHandler.GetDominantElement();
        
            if (dominantElement == PlayerElement.None) {
                StateMachine.SetState(StateMachine.OfRestState);
                return;
            }

            if (dominantElement != _movement.CurrentTargetElement) {
                _movement.SetTargetElement(dominantElement);
            }
        }

        public override void FixedUpdate() {
            _movement.Move();
        }
    }
}