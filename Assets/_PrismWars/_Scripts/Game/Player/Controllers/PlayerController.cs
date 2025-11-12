using System;
using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Services;
using _PrismWars._Scripts.Game.Player.Controllers;
using _PrismWars._Scripts.Game.Player.Controllers.Attack;
using _PrismWars._Scripts.Game.Player.Model;
using _PrismWars._Scripts.Game.Services;
using _PrismWars._Scripts.Game.Services.Client;
using _PrismWars._Scripts.Game.Services.Mono;
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
        
        NetworkVariable<float> _inputMoveDirection = new (
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);
        
        bool _isJumping;
        
        PlayerConfig _config;
        
        NetworkScoreService _networkScoreService;
        
        bool _isInitialized;
        
        ClientMovementPrediction _moveController;
        ClientJumpPrediction _jumpController;
        FlipXController _flipXController;
        AttackMeleeController _attackMeleeController; 
        RangeAttackController _rangeAttackController;
        
        HealthController _healthController;
        ShardController _shardController;
        ProjectileController _projectileController;
        
        CompositeDisposable _disposables = new();
        SpriteRenderer _spriteRenderer;
        
        InputService _inputService;
        
        NetworkVariable<NetworkPlayerData> _playerData = 
            new ();
        
        public NetworkVariable<PlayerElement> PlayerElement { get; } = 
            new (
                default,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Owner);
        
        CatCaptureController _catCaptureController;
        
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
        }

        void FlipX(float previousValue, float newValue) {
            _flipXController.FlipXClientRpc(newValue);
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
                transform,
                _config.meleeDamage);
            
            _projectileController = GetComponent<ProjectileController>();
            
            _rangeAttackController = new RangeAttackController(
                transform,
                _config.playerElement,
                Camera.main,
                _projectileController);

            if (IsOwner) {
                _healthController = GetComponent<HealthController>();
                _healthController.Initialize(_playerData.Value);
                
                _shardController = GetComponent<ShardController>();
                _shardController.Initialize(_playerData.Value.playerElement);
                
                _networkScoreService = ServiceLocator.Singleton.Get<NetworkScoreService>();
                _networkScoreService.Initialize(_playerData.Value.playerElement);
                
                _projectileController.Initialize(
                    _rangeAttackController,
                    _config.maxBulletCount, 
                    3f);
                
                _inputService = ServiceLocator.Singleton.Get<InputService>();
                
                _catCaptureController = GetComponent<CatCaptureController>();
                _catCaptureController.Initialize();
                _inputMoveDirection.OnValueChanged += _catCaptureController.FlipXCat;
                
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
                        if (!_catCaptureController.CatPickedUp)
                            _attackMeleeController.Attack(); 
                    })
                    .AddTo(_disposables);
                _inputService.AttackRange
                    .Subscribe(_ => {
                        if(!_catCaptureController.CatPickedUp)
                            _rangeAttackController.Attack();
                        else
                            _catCaptureController.ThrowCat();
                        }
                    )
                    .AddTo(_disposables);
                _inputService.Interact
                    .Subscribe(_ => _catCaptureController?.PickUpCat())
                    .AddTo(_disposables);
            }

            _isInitialized = true;
        }

        void OnDrawGizmosSelected() {
            if (!IsOwner) return;
            _attackMeleeController.OnDrawGizmosSelected(transform);
        }
        
        public void Dispose() =>
            _disposables?.Dispose();
    }
}