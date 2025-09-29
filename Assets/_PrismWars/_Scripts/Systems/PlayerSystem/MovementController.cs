using UnityEngine;

namespace _PrismWars._Scripts.Player
{
    public class MovementController
    {
        Transform _transform;
        float _speed;
        
        public MovementController(Transform transform, float speed) {
            _transform = transform;
            _speed = speed;
        }
        
        public void Move(Vector2 direction) {
            if (direction.magnitude >= 0.1f) {
                direction.y = 0f;
                _transform.Translate(direction * _speed * Time.deltaTime, Space.World);
            }
        }
    }
}