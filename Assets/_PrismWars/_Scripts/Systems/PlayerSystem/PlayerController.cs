using System;
using System.Collections.Generic;
using _PrismWars._Scripts.Components.Projectile;
using NUnit.Framework;
using R3;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Player
{
    public class PlayerController : NetworkBehaviour, IDisposable {
        PlayerConfig _config;
        
        MovementController _movementController;
        JumpingController _jumpingController;
        FlipXController _flipXController;
        AttackMeleeController _attackMeleeController;
        
        CompositeDisposable _disposables = new();

        Vector3 _direction;
        Rigidbody2D _rb;
        SpriteRenderer _spriteRenderer;
        
        InputService _inputService;
        
        void Start() {
            if (!IsOwner) return;

            _inputService = ServiceLocator.Current.Get<InputService>();
            _config = ServiceLocator.Current.Get<PlayerConfig>();
            _rb = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            
            _movementController = new MovementController(transform, _config.MoveSpeed);
            _jumpingController = new JumpingController(_rb, _config.JumpForce);
            _flipXController = new FlipXController(transform);
            _attackMeleeController = new AttackMeleeController(
                _config.MeleeAttackRange, 
                _config.EnemyLayer, 
                _config.MeleeDamage);
            
            _inputService.MoveInput
                .Subscribe(d => _movementController.Move(d))
                .AddTo(_disposables);
            _inputService.MoveInput
                .Subscribe(d => _flipXController.FlipX(d))
                .AddTo(_disposables);
            _inputService.JumpCommand
                .Subscribe(_ => _jumpingController.Jump())
                .AddTo(_disposables);
            _inputService.AttackMelee
                .Subscribe(_ => _attackMeleeController.MeleeAttack(gameObject))
                .AddTo(_disposables);
        }

        void OnDrawGizmosSelected() {
            _jumpingController.OnDrawGizmosSelected();
            _attackMeleeController.OnDrawGizmosSelected(transform);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) && IsOwner)
            {
                SpawnProjectileRpc(transform.position, transform.right);
            }
        }

        [Rpc(SendTo.Server)]
        void SpawnProjectileRpc(Vector3 position, Vector3 direction) {
            var projectileFactory = ServiceLocator.Current.Get<ProjectileFactory>();
            projectileFactory.Spawn(position, direction);
        }

        public void Dispose() =>
            _disposables?.Dispose();
    }
}