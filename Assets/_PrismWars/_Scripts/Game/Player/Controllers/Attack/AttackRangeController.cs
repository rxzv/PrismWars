using System.Collections.Generic;
using _PrismWars._Scripts.UI;
using _PrismWars._Scripts.UI.Model;
using _PrismWars._Scripts.Utils;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _PrismWars._Scripts.Player {
    public class AttackRangeController {
        const float ATTACK_DISTANCE_FROM_PLAYER = 1f;
        const int MAX_BULLET_COUNT = 10;
        Timer _timer;
        NetworkVariable<float> _maxBulletCount = new();
        Queue<int> _shotQueue = new();
        
        Camera _camera;
        Vector2 _firePoint;
        PlayerElement _playerElement;
        PlayerController _playerController;
        float _rangeAttackCooldown = 3f; // TODO: задать в конфиге
        Transform _playerTransform;
        GameCursorUIService _cursorService;

        NetworkVariable<int> _bulletCountAvailable = new();

        public AttackRangeController(PlayerElement playerElement, int maxBulletCount,
            Timer timer,
            Camera camera, PlayerController playerController) {
            _playerElement = playerElement;
            _camera = camera;
            _playerController = playerController;
            _timer = timer;
            _playerTransform = _playerController.transform;
            _cursorService = ServiceLocator.Singleton.Get<GameCursorUIService>();
            
            SetBulletCountAvailableServerRpc(maxBulletCount);
            _timer.OnTimerComplete += OnTimerComplete;
        }

        void OnTimerComplete() {
            OnTimerCompleteServerRpc();
        }

        [ServerRpc]
        void OnTimerCompleteServerRpc() {
            if (_bulletCountAvailable.Value < _maxBulletCount.Value) {
                _bulletCountAvailable.Value++;
                var index = _shotQueue.Dequeue();
                BulletAvailableRpc(index);
            }

            if (_shotQueue.Count > 0) {
                _timer.StartTimer(_rangeAttackCooldown);
            }
        }
        [Rpc(SendTo.Owner)]
        void BulletAvailableRpc(int index) {
            _cursorService.BulletAvailable(index);
        }

        [ServerRpc]
        void SetBulletCountAvailableServerRpc(int bulletCount) {
            _maxBulletCount.Value = _bulletCountAvailable.Value = bulletCount <= MAX_BULLET_COUNT ? bulletCount : MAX_BULLET_COUNT;
        }

        public void RangeAttack() {
            RangeAttackServerRpc();
        }
        
        [ServerRpc]
        void RangeAttackServerRpc() {
            if (_bulletCountAvailable.Value > 0) {
                _shotQueue.Enqueue(_bulletCountAvailable.Value);
                SpawnProjectileRpc(_bulletCountAvailable.Value);
                _bulletCountAvailable.Value--;
            }

            if (_shotQueue.Count <= 1) {
                _timer.StartTimer(_rangeAttackCooldown);
            }
        }

        [Rpc(SendTo.Owner)]
        void SpawnProjectileRpc(int bulletIndex) {
            _firePoint = GetPositionTowardsMouse(_playerTransform, _camera);
            _playerController.SpawnProjectile(_firePoint, GetShootingDirection(), _playerElement);
            _cursorService.BulletCooldown(bulletIndex);
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