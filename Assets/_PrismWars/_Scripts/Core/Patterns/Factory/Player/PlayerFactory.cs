using _PrismWars._Scripts.Core.Infrastructure.Interfaces;
using _PrismWars._Scripts.Game.Player.Model;
using _PrismWars._Scripts.Player;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Patterns.Factory.Player {
    public abstract class PlayerFactory : Factory {
        protected PlayerFactory(Transform playerPrefab, Vector3[] spawnPoints) 
            : base(playerPrefab, spawnPoints){ }

        public override NetworkObject Spawn(INetworkData data, ulong senderClientId) {
            var spawnId = (int)senderClientId;
            var playerInstance = Object.Instantiate(_prefab, _spawnPoints[spawnId], Quaternion.identity);
            playerInstance.TryGetComponent(out PlayerController networkPlayer);
            playerInstance.TryGetComponent(out NetworkObject nor);
            nor.SpawnWithOwnership(senderClientId);
            var npd = (NetworkPlayerData)data;
            networkPlayer.Initialize(npd);
            return nor;
        }

    }
}