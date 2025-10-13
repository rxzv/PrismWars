using System;
using System.Collections;
using _PrismWars._Scripts.UI.Model;
using R3;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Player
{
    public class PlayerController : NetworkBehaviour, IDisposable {
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

        bool _isInitialized = false;
        
        public NetworkVariable<NetworkPlayerConfig> PlayerConfig = 
            new NetworkVariable<NetworkPlayerConfig>(default, 
                NetworkVariableReadPermission.Everyone, 
                NetworkVariableWritePermission.Server);

        PlayerConfig _config;

        void Awake() {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _rb = GetComponent<Rigidbody2D>();
        }
        
        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            PlayerConfig.OnValueChanged += OnConfigChanged;
        
            if (PlayerConfig.Value.playerName.Length > 0) {
                OnConfigChanged(default, PlayerConfig.Value);
            }
        }
        void OnConfigChanged(NetworkPlayerConfig previous, NetworkPlayerConfig current) {
            _config = ScriptableObject.CreateInstance<PlayerConfig>();
            _config.FromNetworkConfig(current);
        
            ApplyConfig(_config); 
            if (IsOwner) {
                InitializeControllersAndInput();
            }
        }

        void ApplyConfig(PlayerConfig config) {
            _spriteRenderer.sprite = config.sprite;
            gameObject.layer = LayerMask.NameToLayer(_config.playerType.ToString());
        }
        
        void InitializeControllersAndInput() {
            _disposables?.Dispose();
            _disposables = new CompositeDisposable();
            
            _inputService = ServiceLocator.Current.Get<InputService>();
            _attackRangeController = ServiceLocator.Current.Get<AttackRangeController>();
            
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
                    _attackRangeController
                        .AttackRange(new Vector2(transform.position.x + 1, transform.position.y),
                            _config.playerType);
                })
                .AddTo(_disposables);
        }

        void OnDrawGizmosSelected() {
            if (!IsOwner) return;
            _jumpingController.OnDrawGizmosSelected();
            _attackMeleeController.OnDrawGizmosSelected(transform);
        }

        public void Dispose() =>
            _disposables?.Dispose();
    }
}