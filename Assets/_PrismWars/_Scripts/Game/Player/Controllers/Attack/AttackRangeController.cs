using _PrismWars._Scripts.Components.Projectile;
using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _PrismWars._Scripts.Player {
    public class AttackRangeController{
        const float ATTACK_DISTANCE_FROM_PLAYER = 1f;
        
        Camera _camera;
        Vector2 _firePoint;
        PlayerType _playerType;
        PlayerController _playerController;

        public AttackRangeController(PlayerType playerType, Camera camera, PlayerController playerController) {
            _playerType = playerType;
            _camera = camera;
            _playerController = playerController;
        }

        public void RangeAttack(Transform transform) {
            _firePoint = GetPositionTowardsMouse(transform, _camera);
            _playerController.SpawnProjectile(_firePoint, GetShootingDirection(), _playerType);
        }
        Vector2 GetPositionTowardsMouse(Transform transform, Camera camera) {
            Vector2 playerPosition = transform.position;
            Vector2 mousePosition = camera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = (mousePosition - playerPosition).normalized;
            Vector2 targetPosition = playerPosition + direction * ATTACK_DISTANCE_FROM_PLAYER;
        
            return targetPosition;
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
    }
}