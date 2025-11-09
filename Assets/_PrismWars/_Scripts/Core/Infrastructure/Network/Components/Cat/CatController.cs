using System;
using _PrismWars._Scripts.Core.Infrastructure.Network.Components.Cat;
using _PrismWars._Scripts.Game.Services;
using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Infrastructure.Network.Components {
    public class CatController : NetworkBehaviour, IInitializable<NetworkCatData> {
        [SerializeField] string _catTag = "Cat";
        
        const int ADD_SCORE_COUNT = 10;
        
        NetworkScoreService _scoreService;
        CatRespawnService _respawnService;
        
        bool _catIsDespawned = false;

        public NetworkVariable<NetworkCatData> Data = new();
        
        PlayerElement _playerCaptureElement;
        
        public void Initialize(NetworkCatData data) {
            if(IsServer) 
                Data.Value = data;
            OnDataChanged(data,data);
        }

        public void SetCatIsDespawned(bool value) {
            _catIsDespawned = value;
        }
    
        Color GetColorByCatElement(PlayerElement playerElement) {
            return playerElement switch {
                PlayerElement.Fire => Color.red,
                PlayerElement.Ice => Color.blue,
                _ => Color.white
            };
        }

        public override void OnNetworkSpawn() {
            _respawnService = ServiceLocator.Singleton.Get<CatRespawnService>();
            
            Data.OnValueChanged += OnDataChanged;
            if(!IsServer) return;
            _scoreService = ServiceLocator.Singleton.Get<NetworkScoreService>();
            base.OnNetworkSpawn();
        }

        void OnDataChanged(NetworkCatData previous, NetworkCatData current) {
            GetComponent<SpriteRenderer>().color = GetColorByCatElement(Data.Value.catElement);
            gameObject.name = $"{Data.Value.catElement}Cat";
            gameObject.tag = _catTag;
            gameObject.layer = LayerMask.NameToLayer(Data.Value.catElement.ToString());
        }
        
        void OnTriggerEnter2D(Collider2D other) {
            if(!IsOwner) return;
            if(other.CompareTag("Zone") && other.gameObject.layer != gameObject.layer && !_catIsDespawned) {
                Debug.Log("OnTriggerEnter2D");
                _catIsDespawned = true;
                ChangeCatOwnershipServerRpc();
                var no = GetComponent<NetworkObject>();
                _respawnService.CatDespawnRpc(no);
                AddScoreServerRpc();
            }
        }
        [ServerRpc]
        void ChangeCatOwnershipServerRpc() {
            var no = GetComponent<NetworkObject>();
            if(no == null) return;
            no.ChangeOwnership(NetworkManager.Singleton.LocalClientId);
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