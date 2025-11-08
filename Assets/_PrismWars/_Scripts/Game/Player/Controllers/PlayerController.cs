using System;
using _PrismWars._Scripts.Components.Projectile;
using _PrismWars._Scripts.Core.Infrastructure.Network;
using _PrismWars._Scripts.Core.Infrastructure.Network.Components.Shard;
using _PrismWars._Scripts.Game.Services;
using _PrismWars._Scripts.UI.Model;
using _PrismWars._Scripts.Utils;
using R3;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Player {
    public class PlayerController : NetworkBehaviour, IDisposable, IInitializable<NetworkPlayerData> {
        //Both Client And Server Specific
        [SerializeField] int _tickRate = 60;
        
        int _currentTick;
        float _time;
        float _tickTime;
        
        //Server Specific
        [SerializeField] float _maxPositionError = 0.5f;
        [SerializeField] float _maxJumpVelocityError = 2f;
        
        ulong _clientId;
        
        Animator _animator;
        Rigidbody2D _rb;
        
        NetworkVariable<float> _inputMoveDirection = new NetworkVariable<float>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);
        
        bool _isJumping = false;
        
        PlayerConfig _config;
        
        NetworkScoreService _networkScoreService;

        bool _isFlipX = false;
        
        bool _isInitialized = false;
        
        ClientMovementPrediction _moveController;
        ClientJumpPrediction _jumpController;
        FlipXController _flipXController;
        AttackMeleeController _attackMeleeController; 
        AttackRangeController _attackRangeController;
        
        HealthComponent _healthComponent;
        ShardComponent _shardComponent;
        ProjectileComponent _projectileComponent;
        
        CompositeDisposable _disposables = new();
        SpriteRenderer _spriteRenderer;
        
        InputService _inputService;
        ProjectileFactory _projectileFactory;
        
        NetworkVariable<NetworkPlayerData> _playerData = 
            new NetworkVariable<NetworkPlayerData>(default, 
                NetworkVariableReadPermission.Everyone, 
                NetworkVariableWritePermission.Server);
        
        public NetworkVariable<PlayerElement> PlayerElement { get; private set; } = 
            new NetworkVariable<PlayerElement>(
                default,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Owner);
        
        CaptureTheCatComponent _captureTheCatComponent;
        
        public ulong ClientId => _clientId;

        void Awake() {
            _tickTime = 1f / _tickRate;
            
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
            _rb = GetComponent<Rigidbody2D>();
        }

        void Update() {
            _time += Time.deltaTime;
        }
        
        void FixedUpdate() {
            if(!_isInitialized) return;
            if (!IsClient || !IsOwner) return;

            while (_time > _tickTime) {
                _currentTick++;
                _time -= _tickTime;
                
                _moveController.Move(_inputMoveDirection.Value, _currentTick);
                _jumpController.Jump(_currentTick, _isJumping);
                _isJumping = false;
            }
        }

        [Rpc(SendTo.Owner)]
        public void PlayerSetRespawnPositionRpc(Vector3 position) {
            _jumpController.PlayerIsRespawning(_currentTick, position);
            _moveController.PlayerIsRespawning(_currentTick, position);
        }

        [Rpc(SendTo.Everyone)]
        public void PlayerShowRpc() {
            gameObject.SetActive(true);
        }

        public void Initialize(NetworkPlayerData playerConfig) {
            _playerData.Value = playerConfig;
            _projectileFactory = ServiceLocator.Singleton.Get<ProjectileFactory>();
        }

        void FlipX(float previousValue, float newValue) {
            _flipXController.FlipXClientRpc(newValue);
            if(newValue > 0) 
                _isFlipX = false;
            else if(newValue < 0)
                _isFlipX = true;
        }

        public override void OnNetworkSpawn() {
            _playerData.OnValueChanged += OnConfigChanged;
            _inputMoveDirection.OnValueChanged += FlipX;
            _clientId = NetworkManager.Singleton.LocalClientId;
        
            if (_playerData.Value.playerName.Length > 0) {
                OnConfigChanged(default, _playerData.Value);
            }
            base.OnNetworkSpawn();
        }
        void OnConfigChanged(NetworkPlayerData previous, NetworkPlayerData current) {
            _config = ScriptableObject.CreateInstance<PlayerConfig>();
            _config.FromNetworkConfig(current);
        
            ApplyConfig(_config);

            if (!_isInitialized) {
                InitializeControllersAndInput();
            }
        }

        void ApplyConfig(PlayerConfig config) {
            if (IsOwner) {
                PlayerElement.Value = config.playerElement;
            }
            _spriteRenderer.sprite = _config.sprite;
            gameObject.layer = LayerMask.NameToLayer(_config.playerElement.ToString());
        }

        void InitializeControllersAndInput() {
            _disposables?.Dispose();
            _disposables = new CompositeDisposable();
            
            _moveController = new ClientMovementPrediction(
                transform,
                _config.moveSpeed,
                _animator,
                _rb,
                _maxPositionError
            );
            
            _jumpController = new ClientJumpPrediction(
                transform,
                _config.jumpForce,
                _animator,
                _rb,
                _maxJumpVelocityError
            );
            
            _flipXController = new FlipXController(_spriteRenderer);
            
            _attackMeleeController = new AttackMeleeController(
                _config.playerElement,
                _config.meleeAttackRange, 
                _config.enemyLayer,
                _clientId,
                _config.meleeDamage);
            
            _attackRangeController = new AttackRangeController(
                _config.playerElement,
                Camera.main,
                this);

            if (IsOwner) {
                _healthComponent = GetComponent<HealthComponent>();
                _healthComponent.Initialize(_playerData.Value);
                
                _shardComponent = GetComponent<ShardComponent>();
                _shardComponent.Initialize(_playerData.Value.playerElement);
                
                _networkScoreService = ServiceLocator.Singleton.Get<NetworkScoreService>();
                _networkScoreService.Initialize(_playerData.Value.playerElement);
                
                _projectileComponent = GetComponent<ProjectileComponent>();
                _projectileComponent.Initialize(
                    _attackRangeController,
                    _config.maxBulletCount, 
                    3f);
                
                _inputService = ServiceLocator.Singleton.Get<InputService>();
                
                _captureTheCatComponent = GetComponent<CaptureTheCatComponent>();
                
                // Input
                _inputService.MoveInput
                    .Subscribe(d => {
                        Vector3 direction = d.normalized;
                        _inputMoveDirection.Value = direction.x;
                    })
                    .AddTo(_disposables);
                _inputService.JumpCommand
                    .Subscribe(_ => _isJumping = true)
                    .AddTo(_disposables);
                _inputService.AttackMelee
                    .Subscribe(_ => {
                        if (!_captureTheCatComponent.CatPickedUp) ;
                        _attackMeleeController.MeleeAttack(gameObject, _spriteRenderer); 
                    })
                    .AddTo(_disposables);
                _inputService.AttackRange
                    .Subscribe(_ => {
                        if(!_captureTheCatComponent.CatPickedUp)
                            _projectileComponent.RangeAttackServerRpc();
                        else
                            _captureTheCatComponent.ThrowCat(_isFlipX);
                        }
                    )
                    .AddTo(_disposables);
                _inputService.Interact
                    .Subscribe(_ => _captureTheCatComponent?.PickUpCat())
                    .AddTo(_disposables);
            }

            _isInitialized = true;
        }

        void OnDrawGizmosSelected() {
            if (!IsOwner) return;
            _attackMeleeController.OnDrawGizmosSelected(transform);
        }

        public void SpawnProjectile(Vector3 position, Vector3 direction, PlayerElement playerElement) {
            SpawnProjectileRpc(position, direction, playerElement);
        }
        
        [Rpc(SendTo.Server)]
        void SpawnProjectileRpc(Vector3 position, Vector3 direction, PlayerElement playerElement) => 
            _projectileFactory.Spawn(position, direction, playerElement, _clientId);
        
        public void Dispose() =>
            _disposables?.Dispose();
    }
}