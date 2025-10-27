using System;
using _PrismWars._Scripts.Components.Projectile;
using _PrismWars._Scripts.UI.Model;
using R3;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Player {
    public class PlayerController : NetworkBehaviour, IDisposable, IInitializable<NetworkPlayerData> {
        MovementController _movementController;
        JumpingController _jumpingController;
        FlipXController _flipXController;
        AttackMeleeController _attackMeleeController; 
        AttackRangeController _attackRangeController;
        HealthComponent _healthComponent;
        
        CompositeDisposable _disposables = new();
        SpriteRenderer _spriteRenderer;

        Vector3 _direction;
        Rigidbody2D _rb;
        
        InputService _inputService;
        ProjectileFactory _projectileFactory;
        
        NetworkVariable<NetworkPlayerData> _playerData = 
            new NetworkVariable<NetworkPlayerData>(default, 
                NetworkVariableReadPermission.Everyone, 
                NetworkVariableWritePermission.Server);

        PlayerConfig _config;
        
        public NetworkVariable<PlayerElement> PlayerElement { get; private set; } = 
            new NetworkVariable<PlayerElement>(
                default,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Owner);

        void Awake() {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _rb = GetComponent<Rigidbody2D>();
        }

        public void Initialize(NetworkPlayerData playerConfig) {
            _playerData.Value = playerConfig;
            _projectileFactory = ServiceLocator.Singleton.Get<ProjectileFactory>();
        }
        
        public override void OnNetworkSpawn() {
            _playerData.OnValueChanged += OnConfigChanged;
        
            if (_playerData.Value.playerName.Length > 0) {
                OnConfigChanged(default, _playerData.Value);
            }
            base.OnNetworkSpawn();
        }
        void OnConfigChanged(NetworkPlayerData previous, NetworkPlayerData current) {
            _config = ScriptableObject.CreateInstance<PlayerConfig>();
            _config.FromNetworkConfig(current);
        
            ApplyConfig(_config);

            if (IsOwner) {
                InitializeControllersAndInput();
            }
        }

        void ApplyConfig(PlayerConfig config) {
            if (IsOwner) {
                PlayerElement.Value = config.playerElement;
            }
            _spriteRenderer.sprite = config.sprite;
            gameObject.layer = LayerMask.NameToLayer(_config.playerElement.ToString());
        }
        
        void InitializeControllersAndInput() {
            _disposables?.Dispose();
            _disposables = new CompositeDisposable();
            
            _inputService = ServiceLocator.Singleton.Get<InputService>();
            _attackRangeController = new AttackRangeController(_config.playerElement, Camera.main, this);
            
            _movementController = new MovementController(transform, _config.moveSpeed);
            _jumpingController = new JumpingController(_rb, _config.jumpForce);
            _flipXController = new FlipXController(_spriteRenderer);
            _attackMeleeController = new AttackMeleeController(
                _config.meleeAttackRange, 
                _config.enemyLayer,
                _config.meleeDamage);
            _healthComponent = GetComponent<HealthComponent>();
            _healthComponent.Initialize(_playerData.Value);
            
            
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
                        .RangeAttack(transform);
                })
                .AddTo(_disposables);
        }
        
        void OnDrawGizmosSelected() {
            if (!IsOwner) return;
            _jumpingController.OnDrawGizmosSelected();
            _attackMeleeController.OnDrawGizmosSelected(transform);
        }

        public void SpawnProjectile(Vector3 position, Vector3 direction, PlayerElement playerElement) {
            SpawnProjectileRpc(position, direction, playerElement);
        }
        
        [Rpc(SendTo.Server)]
        void SpawnProjectileRpc(Vector3 position, Vector3 direction, PlayerElement playerElement) => 
            _projectileFactory.Spawn(position, direction, playerElement);
        
        public void Dispose() =>
            _disposables?.Dispose();
    }
}