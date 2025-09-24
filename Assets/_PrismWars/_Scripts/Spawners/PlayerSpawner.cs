using System;
using Unity.Netcode;
using Unity.Netcode.Components;
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

        private void Start() {
            // NetworkManager.Singleton.OnClientStarted += SpawnPlayer;
        }

        // void SpawnPlayer() {
        //     SpawnPlayerRpc();
        // }
        // [Rpc(SendTo.Server)]
        // void SpawnPlayerRpc() {
        //     var player = Instantiate(_player, _player.transform.position, Quaternion.identity);
        //     player.GetComponent<NetworkObject>().SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId, true);
        //     OnPlayerSpawned?.Invoke(player);
        // }

        public override void OnNetworkSpawn() {
            if (IsServer) {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            }
        }

        private void OnClientConnected(ulong clientId) {
            if (IsServer) {
                SpawnPlayer(clientId);
            }
        }

        private void OnClientDisconnected(ulong clientId) {
            if (IsServer) {
                // добавить логику удаления игрока
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
        void SpawnPlayerRpc(NetworkObjectReference transform, ulong clientId) {
            OnPlayerSpawned?.Invoke(transform, clientId);
        }

        public override void OnNetworkDespawn() {
            if (IsServer) {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }
    }
}