using System;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts {
    public class PlayerSpawner : NetworkBehaviour {

        #region Singleton

        public static PlayerSpawner Instance;

        private void Awake() {
            if (Instance != null)
                Debug.LogError("There can only be one instance of PlayerSpawner");
            Instance = this;
        }

        #endregion

        [SerializeField] Transform _playerPrefab;

        public Action<NetworkObjectReference, ulong> OnPlayerSpawned;

        public override void OnNetworkSpawn() {
            if (IsServer) {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            }
        }

        private void OnClientConnected(ulong clientId) {
            if (IsServer) {
                SpawnPlayer(clientId);
            }
        }
        private void SpawnPlayer(ulong clientId) {
            if (!IsServer) return;
            
            var currentPlayer = Instantiate(_playerPrefab, _playerPrefab.position, _playerPrefab.rotation);
            NetworkObject networkObject = currentPlayer.GetComponent<NetworkObject>();
            networkObject.SpawnWithOwnership(clientId, true);
            SpawnPlayerRpc(networkObject, clientId);
        }

        [Rpc(SendTo.ClientsAndHost)]
        void SpawnPlayerRpc(NetworkObjectReference transform, ulong clientId) =>
            OnPlayerSpawned?.Invoke(transform, clientId);

        public override void OnNetworkDespawn() {
            if (IsServer) {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            }
        }
    }
}