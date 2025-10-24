using _PrismWars._Scripts.Core.Patterns.Factory;
using _PrismWars._Scripts.UI.Model;
using R3;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts {
    public class NetworkPlayerSpawnService : NetworkBehaviour, IService, IInitializable<PlayerFireFactory, PlayerIceFactory> {
        PlayerFireFactory _fireFactory;
        PlayerIceFactory _iceFactory;
        
        public readonly Subject<(ulong clientId, NetworkObjectReference playerRef)> OnPlayerSpawned = new();
        
        public void Initialize(PlayerFireFactory fireFactory, PlayerIceFactory iceFactory) {
            _fireFactory = fireFactory;
            _iceFactory = iceFactory;
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void SpawnPlayerServerRpc(NetworkPlayerData data, ServerRpcParams rpcParams = default) {
            NetworkObject playerRef;
            
            switch (data.PlayerElement) {
                default:
                    Debug.LogError("Invalid player type");
                    return;
                case PlayerElement.Fire:
                    playerRef = _fireFactory.SpawnPlayer(data, rpcParams.Receive.SenderClientId);
                    break;
                case PlayerElement.Ice:
                    playerRef = _iceFactory.SpawnPlayer(data, rpcParams.Receive.SenderClientId);
                    break;
            }
            
            SpawnPlayerRpc(playerRef, rpcParams.Receive.SenderClientId);
        }

        [Rpc(SendTo.ClientsAndHost)]
        void SpawnPlayerRpc(NetworkObjectReference playerRef, ulong clientId) =>
            OnPlayerSpawned?.OnNext((clientId,playerRef));

    }
}