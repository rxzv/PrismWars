using _PrismWars._Scripts.Game.Services.Client;
using Unity.Netcode;

namespace _PrismWars._Scripts.Core.Infrastructure.Network.Components.BombCart {
    public class BombCartScoreHandler : NetworkBehaviour {
        const int DELIVERY_POINTS = 100;
        NetworkScoreService _scoreService;
        BombCartMovement _movement;

        public override void OnNetworkSpawn() {
            if(!IsServer) return;
            _scoreService = ServiceLocator.Singleton.Get<NetworkScoreService>();
            _movement = GetComponent<BombCartMovement>();
        }

        public void AwardPointsForCartDelivery() {
            if (!IsServer) return;
            var scoringTeam = _movement.CurrentTargetElement;
            _scoreService.AddScore(scoringTeam, DELIVERY_POINTS);
        }
    }
}