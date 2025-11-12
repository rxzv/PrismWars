using _PrismWars._Scripts.Core.Infrastructure.Network.Components.Projectile;
using _PrismWars._Scripts.UI.Model;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _PrismWars._Scripts.Player {
    public class AttackRangeController {
        const float ATTACK_DISTANCE_FROM_PLAYER = 1f;
        
        Camera _camera;
        Vector2 _firePoint;
        PlayerElement _playerElement;
        PlayerController _playerController;
        ProjectileController _projectileController;
        
        Transform _playerTransform;

        public AttackRangeController(PlayerElement playerElement, 
            Camera camera, PlayerController playerController) {
            _playerElement = playerElement;
            _camera = camera;
            _playerController = playerController;
            _playerTransform = _playerController.transform;
        }

        public void SpawnProjectile() {
            _firePoint = GetPositionTowardsMouse(_playerTransform, _camera);
            _playerController.SpawnProjectile(_firePoint, GetShootingDirection(), _playerElement);
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
                mouseRay.GetPoint(50f);
            
            Vector2 direction = (worldPosition - _firePoint).normalized;
            return direction;
        }
    }
}