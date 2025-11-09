using System.Collections.Generic;
using _PrismWars._Scripts.Player;
using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Patterns.Factory {
    public abstract class PlayerFactory : Factory {
        protected PlayerFactory(Transform playerPrefab, List<Transform> spawnPoints) 
            : base(playerPrefab, spawnPoints){ }

        public override NetworkObject Spawn(INetworkData data, ulong senderClientId) {
            int spawnId = (int)senderClientId;
            var playerInstance = Object.Instantiate(_prefab, _spawnPoints[spawnId].position, Quaternion.identity);
            var networkPlayer = playerInstance.GetComponent<PlayerController>();
            var networkObjectReference = playerInstance.GetComponent<NetworkObject>();
            playerInstance.GetComponent<NetworkObject>().SpawnWithOwnership(senderClientId);
            networkPlayer.Initialize((NetworkPlayerData)data);
            return networkObjectReference;
        }

    }
}