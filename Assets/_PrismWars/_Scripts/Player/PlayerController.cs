using R3;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Player
{
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] PlayerConfig _config;
        
        MovementController _movementController;
        JumpingController _jumpingController;
        
        CompositeDisposable _disposables = new();

        Vector3 _direction;
        Rigidbody2D _rb;

        private void Start() {
            _rb = GetComponent<Rigidbody2D>();
            _movementController = new MovementController(transform, _config.MoveSpeed);
            _jumpingController = new JumpingController(_rb, _config.JumpForce);
            if (!IsOwner) return;
            InputSystem.Instance.MoveInput
                .Subscribe(d => _direction = d)
                .AddTo(_disposables);
            InputSystem.Instance.JumpCommand
                .Subscribe(_ => _jumpingController.Jump())
                .AddTo(_disposables);
        }
        
        void Update() {
            if (_direction.magnitude >= 0.1f) 
                _movementController.Move(_direction);
        }
    }
}