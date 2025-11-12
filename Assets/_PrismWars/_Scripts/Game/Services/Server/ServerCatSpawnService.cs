using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Services;
using _PrismWars._Scripts.Core.Infrastructure.Network.Components.Cat;
using _PrismWars._Scripts.Core.Patterns.Factory.Cat;
using _PrismWars._Scripts.Game.Player.Model;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Game.Services.Server {
    public class ServerCatSpawnService : NetworkBehaviour, IService, IInitializable<CatFireFactory, CatIceFactory> {
        CatFireFactory _fireFactory;
        CatIceFactory _iceFactory;
        
        public void Initialize(CatFireFactory catFireFactory, CatIceFactory catIceFactory) {
            _fireFactory = catFireFactory;
            _iceFactory = catIceFactory;
        }
        [Rpc(SendTo.Server)]
        public void SpawnCatRpc(NetworkCatData data, ulong clientId) {
            if(!IsServer) return;
            
            switch (data.catElement) {
                case PlayerElement.None:
                default:
                    Debug.LogError("Invalid cat type");
                    return;
                case PlayerElement.Fire:
                    _fireFactory.Spawn(data, clientId);
                    break;
                case PlayerElement.Ice:
                    _iceFactory.Spawn(data, clientId);
                    break;
            }
        }
    }
}