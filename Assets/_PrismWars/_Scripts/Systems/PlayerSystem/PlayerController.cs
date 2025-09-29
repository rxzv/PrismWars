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
        
        CompositeDisposable _disposables = new();

        Vector3 _direction;
        Rigidbody2D _rb;
        SpriteRenderer _spriteRenderer;
        
        void Start() {
            if (!IsOwner) return;

            _config = ServiceLocator.Current.Get<PlayerConfig>();
            _rb = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            
            _movementController = new MovementController(transform, _config.MoveSpeed);
            _jumpingController = new JumpingController(_rb, _config.JumpForce);
            _flipXController = new FlipXController(_spriteRenderer);
            
            ServiceLocator.Current.Get<InputService>().MoveInput
                .Subscribe(d => _movementController.Move(d))
                .AddTo(_disposables);
            ServiceLocator.Current.Get<InputService>().JumpCommand
                .Subscribe(_ => _jumpingController.Jump())
                .AddTo(_disposables);
            ServiceLocator.Current.Get<InputService>().MoveInput
                .Subscribe(d => _flipXController.FlipX(d))
                .AddTo(_disposables);
        }

        public void Dispose() =>
            _disposables?.Dispose();
    }
}