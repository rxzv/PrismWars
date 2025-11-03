using System.Collections.Generic;
using _PrismWars._Scripts.Player;
using _PrismWars._Scripts.UI;
using _PrismWars._Scripts.Utils;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Components.Projectile {
    public class ProjectileComponent : NetworkBehaviour, IInitializable<AttackRangeController, int, float> {
        const int MAX_BULLET_COUNT = 10;
        const float MAX_RANGE_ATTACK_COOLDOWN = 3f;
        
        Timer _timer;
        NetworkVariable<float> _maxBulletCount = new();
        Stack<int> _shotStack = new();
        GameCursorUIService _cursorService;

        AttackRangeController _attackRangeController;
        
        float _rangeAttackCooldown;
        
        NetworkVariable<int> _bulletCountAvailable = new();

        public override void OnNetworkSpawn() {
            if (IsServer) {
                _timer = new Timer();
                _timer.OnTimerComplete += OnTimerComplete;
            }

            if (IsOwner) {
                _cursorService = ServiceLocator.Singleton.Get<GameCursorUIService>();
            }
            
            base.OnNetworkSpawn();
        }

        public void Initialize(AttackRangeController attackRangeController, int maxBulletCount, float rangeAttackCooldown) {
            _attackRangeController = attackRangeController;
            SetBulletCountAvailableServerRpc(maxBulletCount, rangeAttackCooldown);
        }

        void OnTimerComplete() {
            if(!IsServer) return;
            if (_bulletCountAvailable.Value < _maxBulletCount.Value) {
                _bulletCountAvailable.Value++;
                Debug.Log($"_shotQueue.Dequeue({_bulletCountAvailable.Value});");
                var index = _shotStack.Pop();
                BulletAvailableRpc(index);
            }

            if (_shotStack.Count > 0) {
                _timer.StartTimer(_rangeAttackCooldown);
            }
        }

         void Update() {
            _timer?.Update();
        }

        [Rpc(SendTo.Owner)]
        void BulletAvailableRpc(int index) {
            _cursorService.BulletAvailable(index);
        }

        [ServerRpc]
        void SetBulletCountAvailableServerRpc(int bulletCount, float rangeAttackCooldown) {
            _maxBulletCount.Value = _bulletCountAvailable.Value = bulletCount <= MAX_BULLET_COUNT 
                ? bulletCount 
                : MAX_BULLET_COUNT;
            _rangeAttackCooldown = rangeAttackCooldown <= MAX_RANGE_ATTACK_COOLDOWN
                ? rangeAttackCooldown
                : MAX_RANGE_ATTACK_COOLDOWN;
        }
        
        [ServerRpc]
        public void RangeAttackServerRpc() {
            if (_bulletCountAvailable.Value > 0) {
                _shotStack.Push(_bulletCountAvailable.Value);
                Debug.Log($"_shotQueue.Enqueue({_bulletCountAvailable.Value});");
                SpawnProjectileRpc(_bulletCountAvailable.Value);
                _bulletCountAvailable.Value--;
            }

            if (_shotStack.Count <= 1) {
                _timer.StartTimer(_rangeAttackCooldown);
            }
        }

        [Rpc(SendTo.Owner)]
        void SpawnProjectileRpc(int bulletIndex) {
            _attackRangeController.SpawnProjectile();
            _cursorService.BulletCooldown(bulletIndex);
        }
        
    }
}