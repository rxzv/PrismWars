namespace _PrismWars._Scripts.Core.Infrastructure.Network.Components.BombCart {
    public class BombCartInZoneState : BombCartBaseState {
        public override BombCartState StateType => BombCartState.InZone;
    
        BombCartScoreHandler _scoreHandler;

        public BombCartInZoneState(BombCartStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter() {
            var movement = StateMachine.GetComponent<BombCartMovement>();
            _scoreHandler = StateMachine.GetComponent<BombCartScoreHandler>();
        
            movement.Stop();
            _scoreHandler.AwardPointsForCartDelivery();
        }
    }
}