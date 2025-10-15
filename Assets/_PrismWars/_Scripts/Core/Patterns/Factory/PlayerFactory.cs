using System.Collections.Generic;
using _PrismWars._Scripts.Player;
using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Patterns.Factory {
    public abstract class PlayerFactory {
        protected Transform _playerPrefab;
        protected List<Transform> _spawnPoints;
        
        public PlayerFactory(Transform playerPrefab, List<Transform> spawnPoints) {
            _playerPrefab =  playerPrefab;
            _spawnPoints = spawnPoints;
        }

        public NetworkObject SpawnPlayer(NetworkPlayerData data, ulong senderClientId) {
            int spawnId = (int)senderClientId;
            var playerInstance = Object.Instantiate(_playerPrefab, _spawnPoints[spawnId].position, Quaternion.identity);
            var networkPlayer = playerInstance.GetComponent<PlayerController>();
            var networkObjectReference = playerInstance.GetComponent<NetworkObject>();
            playerInstance.GetComponent<NetworkObject>().SpawnWithOwnership(senderClientId);
            networkPlayer.Initialize(data);
            return networkObjectReference;
        }

    }
}