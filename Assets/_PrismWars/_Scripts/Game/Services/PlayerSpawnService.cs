using System.Collections.Generic;
using _PrismWars._Scripts.Player;
using _PrismWars._Scripts.UI.Model;
using R3;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts {
    public class PlayerSpawnService : NetworkBehaviour, IService, IInitializable<Transform, List<Transform>, List<Transform>> {
        List<Transform> _fireSpawnPoints;
        List<Transform> _iceSpawnPoints;
        
        Transform _playerPrefab;
        
        public readonly Subject<(ulong clientId, NetworkObjectReference playerRef)> OnPlayerSpawned = new();
        
        public void Initialize(Transform playerPrefab, List<Transform> iceSpawnPoints, List<Transform> fireSpawnPoints) {
            _playerPrefab = playerPrefab;
            _iceSpawnPoints = iceSpawnPoints;
            _fireSpawnPoints = fireSpawnPoints;
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void SpawnPlayerServerRpc(NetworkPlayerData data, ServerRpcParams rpcParams = default){
            Transform spawnPoint;
            int index;
            switch (data.playerType) {
                default:
                case PlayerType.Fire:
                    index = (int)rpcParams.Receive.SenderClientId;
                    spawnPoint = _fireSpawnPoints[index];
                    break;
                case PlayerType.Ice:
                    index = (int)rpcParams.Receive.SenderClientId;
                    spawnPoint = _iceSpawnPoints[index];
                    break;
            }
            var playerInstance = Instantiate(_playerPrefab, spawnPoint.position, Quaternion.identity);
            var networkPlayer = playerInstance.GetComponent<PlayerController>();
            var networkObjectReference = playerInstance.GetComponent<NetworkObject>();
            playerInstance.GetComponent<NetworkObject>().SpawnWithOwnership(
                rpcParams.Receive.SenderClientId);
        
            networkPlayer.Initialize(data);
            SpawnPlayerRpc(networkObjectReference, rpcParams.Receive.SenderClientId);
        }

        [Rpc(SendTo.ClientsAndHost)]
        void SpawnPlayerRpc(NetworkObjectReference transform, ulong clientId) =>
            OnPlayerSpawned?.OnNext((clientId,transform));

    }
}