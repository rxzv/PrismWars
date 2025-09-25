using System;
using R3;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts {
    public class PlayerSpawner : NetworkBehaviour, IDisposable {

        #region Singleton

        public static PlayerSpawner Instance;

        private void Awake() {
            if (Instance != null)
                Debug.LogError("There can only be one instance of PlayerSpawner");
            Instance = this;
        }

        #endregion

        [SerializeField] Transform _playerPrefab;

        public readonly Subject<(ulong clientId, NetworkObjectReference playerRef)> OnPlayerSpawned = new();
        readonly CompositeDisposable _disposables = new();
        
        public override void OnNetworkSpawn() {
            if (IsServer) {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            }
        }

        private void OnClientConnected(ulong clientId) {
            if (IsServer) {
                Observable.NextFrame()
                    .Subscribe(_ => SpawnPlayer(clientId))
                    .AddTo(_disposables);
            }
        }
        private void SpawnPlayer(ulong clientId) {
            if (!IsServer) return;
            
            var currentPlayer = Instantiate(_playerPrefab);
            NetworkObject networkObject = currentPlayer.GetComponent<NetworkObject>();
            networkObject.SpawnWithOwnership(clientId, true);
            SpawnPlayerRpc(networkObject, clientId);
        }

        [Rpc(SendTo.ClientsAndHost)]
        void SpawnPlayerRpc(NetworkObjectReference transform, ulong clientId) =>
            OnPlayerSpawned?.OnNext((clientId,transform));

        public void Dispose()
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            _disposables.Dispose();
        }
    }
}