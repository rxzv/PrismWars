using UnityEngine;

namespace _PrismWars._Scripts.Player
{ 
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Configs/Player Config")]
    public class PlayerConfig : ScriptableObject, IService {
        [SerializeField] float _moveSpeed = 10f;
        [SerializeField] float _jumpForce = 10f;
        [SerializeField] float _maxHealth = 100f;
        
        public float JumpForce => _jumpForce;
        public float MoveSpeed => _moveSpeed;
        public float MaxHealth => _maxHealth;
    }
}