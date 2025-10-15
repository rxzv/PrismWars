using System.Collections.Generic;
using _PrismWars._Scripts.Core.Patterns.Factory;
using _PrismWars._Scripts.Player;
using _PrismWars._Scripts.UI.Model;
using R3;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts {
    public class PlayerSpawnService : NetworkBehaviour, IService, IInitializable<Transform, List<Transform>, List<Transform>> {
        List<Transform> _fireSpawnPoints;
        List<Transform> _iceSpawnPoints;
        
        PlayerFactory _playerFactory;
        
        public readonly Subject<(ulong clientId, NetworkObjectReference playerRef)> OnPlayerSpawned = new();
        
        public void Initialize(Transform playerPrefab, List<Transform> iceSpawnPoints, List<Transform> fireSpawnPoints) {
            _iceSpawnPoints = iceSpawnPoints;
            _fireSpawnPoints = fireSpawnPoints;
            _playerFactory = new PlayerFactory(playerPrefab);
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void SpawnPlayerServerRpc(NetworkPlayerData data, ServerRpcParams rpcParams = default) {
            NetworkObject playerRef;
            Transform spawnPoint;
            int index;
            
            switch (data.playerType) {
                default:
                    return;
                case PlayerType.Fire:
                    index = (int)rpcParams.Receive.SenderClientId;
                    spawnPoint = _fireSpawnPoints[index];
                    playerRef = _playerFactory.SpawnPlayer(data,  spawnPoint, rpcParams.Receive.SenderClientId);
                    break;
                case PlayerType.Ice:
                    index = (int)rpcParams.Receive.SenderClientId;
                    spawnPoint = _iceSpawnPoints[index];
                    playerRef = _playerFactory.SpawnPlayer(data,  spawnPoint, rpcParams.Receive.SenderClientId);
                    break;
            }
            
            SpawnPlayerRpc(playerRef, rpcParams.Receive.SenderClientId);
        }

        [Rpc(SendTo.ClientsAndHost)]
        void SpawnPlayerRpc(NetworkObjectReference playerRef, ulong clientId) =>
            OnPlayerSpawned?.OnNext((clientId,playerRef));

    }
}