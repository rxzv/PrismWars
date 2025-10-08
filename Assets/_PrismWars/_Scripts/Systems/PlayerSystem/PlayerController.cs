using System;
using System.Collections;
using _PrismWars._Scripts.UI.Model;
using R3;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Player
{
    public class PlayerController : NetworkBehaviour, IDisposable, IInitializable<PlayerConfig> {
        MovementController _movementController;
        JumpingController _jumpingController;
        FlipXController _flipXController;
        AttackMeleeController _attackMeleeController; 
        AttackRangeController _attackRangeController;
        
        CompositeDisposable _disposables = new();
        SpriteRenderer _spriteRenderer;

        Vector3 _direction;
        Rigidbody2D _rb;
        
        PlayerConfig _config;
        InputService _inputService;

        bool _isInitialized = false;

        public void Initialize(PlayerConfig config) {
            _config = config;
            _isInitialized = true;
        }

        IEnumerator WaitForInitialization() {
            while (!_isInitialized || !_config)
                yield return null;
            Debug.Log("Config initialized: PlayerController");
        }
        void Start() {
            if (!IsOwner) return;
            StartCoroutine(WaitForInitialization());
            
            _inputService = ServiceLocator.Current.Get<InputService>();
            _rb = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _attackRangeController = ServiceLocator.Current.Get<AttackRangeController>();

            _spriteRenderer.sprite = _config.sprite;
            gameObject.layer = LayerMask.NameToLayer(_config.playerType.ToString());
            
            _movementController = new MovementController(transform, _config.moveSpeed);
            _jumpingController = new JumpingController(_rb, _config.jumpForce);
            _flipXController = new FlipXController(_spriteRenderer);
            _attackMeleeController = new AttackMeleeController(
                _config.meleeAttackRange, 
                _config.enemyLayer, 
                _config.meleeDamage);
            
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
                        _attackRangeController
                            .AttackRange(new Vector2(transform.position.x + 1, transform.position.y), _config.playerType);
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