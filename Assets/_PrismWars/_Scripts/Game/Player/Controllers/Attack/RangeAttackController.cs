using _PrismWars._Scripts.Core.Infrastructure.Network.Components.Projectile;
using _PrismWars._Scripts.UI.Model;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _PrismWars._Scripts.Game.Player.Controllers.Attack {
    public class RangeAttackController : AttackController {
        const float ATTACK_DISTANCE_FROM_PLAYER = 1f;
        
        Camera _camera;
        ProjectileController _projectileController;

        public RangeAttackController(Transform playerTransform, PlayerElement playerElement, 
            Camera camera, ProjectileController projectileController) : base(playerTransform, playerElement){
            _camera = camera;
            _projectileController = projectileController;
        } 

        public void Shoot() {
            _attackPos = GetPositionTowardsMouse(_playerTransform, _camera);
            _projectileController.SpawnProjectile(_attackPos, GetShootingDirection(), _playerElement);
        }
        
        public override void Attack() {
            _projectileController.CheckProjectilesForTheShot();
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

            Vector3 worldPosition = Physics.Raycast(mouseRay, out RaycastHit hit, 100f) ? hit.point :
                mouseRay.GetPoint(50f);
            
            Vector3 direction = (worldPosition - _attackPos).normalized;
            return direction;
        }

    }
}