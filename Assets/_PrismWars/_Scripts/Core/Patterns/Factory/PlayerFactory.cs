using _PrismWars._Scripts.Player;
using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Patterns.Factory {
    public class PlayerFactory {
        readonly Transform _playerPrefab;

        public PlayerFactory(Transform playerPrefab) {
            _playerPrefab = playerPrefab;
        }

        public NetworkObject SpawnPlayer(NetworkPlayerData data, Transform spawnPoint, ulong senderClientId) {
            var playerInstance = Object.Instantiate(_playerPrefab, spawnPoint.position, Quaternion.identity);
            var networkPlayer = playerInstance.GetComponent<PlayerController>();
            var networkObjectReference = playerInstance.GetComponent<NetworkObject>();
            playerInstance.GetComponent<NetworkObject>().SpawnWithOwnership(senderClientId);
            networkPlayer.Initialize(data);
            return networkObjectReference;
        }

    }
}