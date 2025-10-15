using _PrismWars._Scripts.Player;
using _PrismWars._Scripts.UI.Model;
using R3;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts {
    public class PlayerSpawnService : NetworkBehaviour, IService, IInitializable<Transform> {
        
        Transform _playerPrefab;
        
        public readonly Subject<(ulong clientId, NetworkObjectReference playerRef)> OnPlayerSpawned = new();
        
        public void Initialize(Transform playerPrefab) {
            _playerPrefab = playerPrefab;
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void SpawnPlayerServerRpc(NetworkPlayerData data, ServerRpcParams rpcParams = default){
            var playerInstance = Instantiate(_playerPrefab);
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