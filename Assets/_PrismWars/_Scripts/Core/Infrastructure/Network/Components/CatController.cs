using System;
using _PrismWars._Scripts.Game.Services;
using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Infrastructure.Network.Components {
    public class CatController : NetworkBehaviour {
        const int ADD_SCORE_COUNT = 10;
        NetworkScoreService _scoreService;
        
        PlayerElement _playerCaptureElement;
        
        public override void OnNetworkSpawn() {
            if(!IsServer) return;
            _scoreService = ServiceLocator.Singleton.Get<NetworkScoreService>();
            base.OnNetworkSpawn();
        }

        void OnTriggerEnter2D(Collider2D other) {
            if(!IsOwner) return;
            if(other.CompareTag("Zone") && other.gameObject.layer != gameObject.layer) {
                gameObject.SetActive(false);
                AddScoreServerRpc();
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void SetPlayerCaptureElementServerRpc(PlayerElement element) {
            _playerCaptureElement = element;
        }

        [ServerRpc(RequireOwnership = false)]
        void AddScoreServerRpc() {
            _scoreService.AddScore(_playerCaptureElement, ADD_SCORE_COUNT);
        }

    }
}