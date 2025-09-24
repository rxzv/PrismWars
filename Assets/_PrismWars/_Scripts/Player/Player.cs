using System;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Player
{
    public class Player : NetworkBehaviour
    {
        [SerializeField] PlayerConfig _config;
        
        public ulong Id { get; private set; }
        
        MovementController _movementController;
        JumpingController _jumpingController;
        
        bool _isJumping = false;

        private void Start() {
            Id = NetworkManager.Singleton.LocalClientId;
            _movementController = new MovementController(transform, _config.MoveSpeed);
            _jumpingController = new JumpingController(GetComponent<Rigidbody2D>(), _config.JumpForce);
            InputManager.Instance.OnJumpStarted += JumpStarted;
        }
        
        void Update() {
            if (!IsOwner) return;
            
            _movementController.Move(SetDirection());
        }

        void FixedUpdate() {
            if (!IsOwner) return;
            
            if (_isJumping) {
                _jumpingController.Jump();
                _isJumping = false;
            }
        }

        void JumpStarted() => _isJumping = true;

        Vector2 SetDirection() => InputManager.Instance.PlayerInputActions.Player.Move.ReadValue<Vector2>().normalized;
    }
}