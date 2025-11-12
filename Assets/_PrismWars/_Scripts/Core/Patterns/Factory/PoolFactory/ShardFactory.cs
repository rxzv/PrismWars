using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Services;
using _PrismWars._Scripts.Core.Infrastructure.Network.Components.Shard;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Patterns.Factory.PoolFactory {
    [RequireComponent(typeof(NetworkObject))]
    public class ShardFactory : NetworkPoolGenericFactory<Shard>, IService {
        
    }
}