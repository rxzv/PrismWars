using System;
using UnityEngine;

namespace _PrismWars._Scripts.Player
{
    public class Player : MonoBehaviour
    {
        [SerializeField] PlayerConfig _config;
        MovementController _movementController;
        JumpingController _jumpingController;

        private void Start() {
            _movementController = new MovementController(transform, _config.MoveSpeed);
            _jumpingController = new JumpingController(GetComponent<Rigidbody2D>(), _config.JumpForce);
        }
        
        private void Update() {
            _movementController.Move(SetDirection());
        }

        Vector2 SetDirection() => InputManager.Instance.PlayerInputActions.Player.Move.ReadValue<Vector2>().normalized;
    }
}