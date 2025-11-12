using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Services;
using _PrismWars._Scripts.Core.Infrastructure.Network.Components.ProjectileComponent;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Patterns.Factory.PoolFactory.ProjectileFactory {
    [RequireComponent(typeof(NetworkObject))]
    public class ProjectileFactory : NetworkPoolGenericFactory<Projectile>, IService {
    }
}