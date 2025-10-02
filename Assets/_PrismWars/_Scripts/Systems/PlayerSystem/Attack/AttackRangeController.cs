using _PrismWars._Scripts.Components.Projectile;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _PrismWars._Scripts.Player {
    [RequireComponent(typeof(NetworkObject))]
    public class AttackRangeController : NetworkBehaviour, IService, IInitializable {
        ProjectileFactory _projectileFactory;
        Camera _camera;
        Vector2 _firePoint;

        public void Initialize() {
            _projectileFactory = ServiceLocator.Current.Get<ProjectileFactory>();
        }
        
        void Start() => _camera = Camera.main;

        public void AttackRange(Vector2 position) {
            _firePoint = position;
            SpawnProjectileRpc(position, GetShootingDirection());
        }
        
        Vector2 GetShootingDirection() {
            Ray mouseRay = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            Vector2 worldPosition = Physics.Raycast(mouseRay, out RaycastHit hit, 100f) ? hit.point :
                // Если луч ни во что не попал, используем точку на дальней дистанции
                mouseRay.GetPoint(50f);

            // Направление от точки выстрела к цели
            Vector2 direction = (worldPosition - _firePoint).normalized;
            return direction;
        }
        
        [Rpc(SendTo.Server)]
        void SpawnProjectileRpc(Vector3 position, Vector3 direction) => _projectileFactory.Spawn(position, direction);
    }
}