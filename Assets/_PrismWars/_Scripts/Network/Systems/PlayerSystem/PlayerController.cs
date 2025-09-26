using R3;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Player
{
    public class PlayerController : NetworkBehaviour {
        [SerializeField] PlayerConfig _config;
        
        MovementController _movementController;
        JumpingController _jumpingController;
        
        CompositeDisposable _disposables = new();

        Vector3 _direction;
        Rigidbody2D _rb;
        
        void Start() {
            if (!IsOwner) return;
            _rb = GetComponent<Rigidbody2D>();
            
            _movementController = new MovementController(transform, _config.MoveSpeed);
            _jumpingController = new JumpingController(_rb, _config.JumpForce);
            
            ServiceLocator.Current.Get<InputService>().MoveInput
                .Subscribe(d => _movementController.Move(d))
                .AddTo(_disposables);
            ServiceLocator.Current.Get<InputService>().JumpCommand
                .Subscribe(_ => _jumpingController.Jump())
                .AddTo(_disposables);
        }
    }
}