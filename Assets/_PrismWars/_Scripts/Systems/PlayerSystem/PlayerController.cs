using System;
using R3;
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
        AttackRangeController _attackRangeController;
        
        CompositeDisposable _disposables = new();
        SpriteRenderer _spriteRenderer;

        Vector3 _direction;
        Rigidbody2D _rb;
        
        InputService _inputService;
        
        void Start() {
            if (!IsOwner) return;

            _inputService = ServiceLocator.Current.Get<InputService>();
            _config = ServiceLocator.Current.Get<PlayerConfig>();
            _rb = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _attackRangeController = ServiceLocator.Current.Get<AttackRangeController>();
            
            _movementController = new MovementController(transform, _config.MoveSpeed);
            _jumpingController = new JumpingController(_rb, _config.JumpForce);
            _flipXController = new FlipXController(_spriteRenderer);
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
                .Subscribe(_ => _attackMeleeController.MeleeAttack(gameObject, _spriteRenderer))
                .AddTo(_disposables);
            _inputService.AttackRange
                .Subscribe(_ => {
                    if (IsOwner)
                        _attackRangeController.AttackRange(new Vector2(transform.position.x + 1, transform.position.y));
                })
                .AddTo(_disposables);
        }

        void OnDrawGizmosSelected() {
            _jumpingController.OnDrawGizmosSelected();
            _attackMeleeController.OnDrawGizmosSelected(transform);
        }

        public void Dispose() =>
            _disposables?.Dispose();
    }
}