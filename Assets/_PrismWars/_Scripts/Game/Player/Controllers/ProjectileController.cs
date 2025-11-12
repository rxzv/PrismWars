using System.Collections.Generic;
using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Services;
using _PrismWars._Scripts.Core.Patterns.Factory.ProjectileFactory;
using _PrismWars._Scripts.Game.Player.Controllers.Attack;
using _PrismWars._Scripts.Player;
using _PrismWars._Scripts.UI;
using _PrismWars._Scripts.UI.Model;
using _PrismWars._Scripts.Utils;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Infrastructure.Network.Components.Projectile {
    public class ProjectileController : NetworkBehaviour, IInitializable<AttackRangeController, int, float> {
        const int MAX_BULLET_COUNT = 10;
        const float MAX_RANGE_ATTACK_COOLDOWN = 3f;
        
        Timer _timer;
        NetworkVariable<float> _maxBulletCount = new();
        Stack<int> _shotStack = new();
        GameCursorUIService _cursorService;

        AttackRangeController _attackRangeController;
        ProjectileFactory _projectileFactory;
        
        float _rangeAttackCooldown;
        ulong _clientId;
        
        NetworkVariable<int> _bulletCountAvailable = new();

        public override void OnNetworkSpawn() {
            _clientId = NetworkManager.Singleton.LocalClientId;
            if (IsServer) {
                _timer = new Timer();
                _timer.OnTimerComplete += OnTimerComplete;
                _projectileFactory = ServiceLocator.Singleton.Get<ProjectileFactory>();
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

        [Rpc(SendTo.Server)]
        void SetBulletCountAvailableServerRpc(int bulletCount, float rangeAttackCooldown) {
            _maxBulletCount.Value = _bulletCountAvailable.Value = bulletCount <= MAX_BULLET_COUNT 
                ? bulletCount 
                : MAX_BULLET_COUNT;
            _rangeAttackCooldown = rangeAttackCooldown <= MAX_RANGE_ATTACK_COOLDOWN
                ? rangeAttackCooldown
                : MAX_RANGE_ATTACK_COOLDOWN;
        }

        public void CheckProjectilesForTheShot() {
            CheckProjectilesForTheShotServerRpc();
        }
        
        [Rpc(SendTo.Server)]
        void CheckProjectilesForTheShotServerRpc() {
            if (_bulletCountAvailable.Value > 0) {
                _shotStack.Push(_bulletCountAvailable.Value);
                ShootRpc(_bulletCountAvailable.Value);
                _bulletCountAvailable.Value--;
            }

            if (_shotStack.Count <= 1) {
                _timer.StartTimer(_rangeAttackCooldown);
            }
        }
        public void SpawnProjectile(Vector3 position, Vector3 direction, PlayerElement playerElement) {
            SpawnProjectileRpc(position, direction, playerElement, _clientId);
        }
        
        [Rpc(SendTo.Server)]
        void SpawnProjectileRpc(Vector3 position, Vector3 direction, PlayerElement playerElement,  ulong clientId) => 
            _projectileFactory.Spawn(position, direction, playerElement, clientId);


        [Rpc(SendTo.Owner)]
        void ShootRpc(int bulletIndex) {
            _attackRangeController.Shoot();
            _cursorService.BulletCooldown(bulletIndex);
        }
        
    }
}