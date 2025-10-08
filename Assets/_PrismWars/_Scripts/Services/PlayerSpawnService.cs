using System;
using System.Collections.Generic;
using _PrismWars._Scripts.Player;
using _PrismWars._Scripts.UI.Model;
using R3;
using Unity.Netcode;
using Unity.Networking.Transport;
using UnityEngine;

namespace _PrismWars._Scripts {
    public class PlayerSpawnService : NetworkBehaviour, IService,IDisposable, IInitializable<Transform> {
        [SerializeField] List<PlayerConfig> _availableConfigs;
        // [SerializeField] FirePlayerFactory firePlayerFactory;
        // [SerializeField] IcePlayerFactory icePlayerFactory;
        
        private static Dictionary<int, PlayerConfig> _playerConfigs = new();
        private static int _nextConnectionId = 0;
        
        Transform _playerPrefab;
        
        public readonly Subject<(ulong clientId, NetworkObjectReference playerRef)> OnPlayerSpawned = new();
        readonly CompositeDisposable _disposables = new();
        
        public void SetPlayerConfig(PlayerConfig config) {
            if (!_playerConfigs.ContainsValue(config)) 
                _playerConfigs[_nextConnectionId] = config;
        }
        
        public void Initialize(Transform playerPrefab) {
            _playerPrefab = playerPrefab;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }

        void OnClientConnected(ulong clientId) {
            if (IsServer) {
                Observable.NextFrame()
                    .Subscribe(_ => SpawnPlayer(clientId))
                    .AddTo(_disposables);
            }
        }
        void SpawnPlayer(ulong clientId) {
            if (!IsServer) return;
         
            var currentPlayer = Instantiate(_playerPrefab);
            
            var healthComponent = currentPlayer.GetComponent<HealthComponent>();
            healthComponent.Initialize(_playerConfigs[_nextConnectionId]);
            
            NetworkObject networkObject = currentPlayer.GetComponent<NetworkObject>();
            networkObject.SpawnWithOwnership(clientId, true);
            InitializePlayerRpc(networkObject, clientId);
            SpawnPlayerRpc(networkObject, clientId);
        }

        [Rpc(SendTo.ClientsAndHost)]
        void InitializePlayerRpc(NetworkObjectReference networkObjectRef, ulong clientId) {
            if (NetworkManager.Singleton.LocalClientId != clientId) return;

            networkObjectRef.TryGet(out NetworkObject networkObject);
            var playerController = networkObject.gameObject.GetComponent<PlayerController>();
            playerController.Initialize(_playerConfigs[_nextConnectionId]);
        }

        [Rpc(SendTo.ClientsAndHost)]
        void SpawnPlayerRpc(NetworkObjectReference transform, ulong clientId) =>
            OnPlayerSpawned?.OnNext((clientId,transform));

        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}