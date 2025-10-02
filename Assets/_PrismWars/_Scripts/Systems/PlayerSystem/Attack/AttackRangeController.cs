using _PrismWars._Scripts.Components.Projectile;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Player {
    [RequireComponent(typeof(NetworkObject))]
    public class AttackRangeController : NetworkBehaviour, IService, IInitializable {
        ProjectileFactory _projectileFactory;

        public void Initialize() {
            _projectileFactory = ServiceLocator.Current.Get<ProjectileFactory>();
        }

        public void AttackRange(Vector2 position, Vector2 direction) => SpawnProjectileRpc(position, direction);
        
        [Rpc(SendTo.Server)]
        void SpawnProjectileRpc(Vector3 position, Vector3 direction) => _projectileFactory.Spawn(position, direction);
    }
}