using UnityEngine;

namespace _PrismWars._Scripts.Player
{ 
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Configs/Player Config")]
    public class PlayerConfig : ScriptableObject
    {
        [SerializeField] float _moveSpeed = 10f;
        [SerializeField] float _jumpForce = 10f;
        
        public float JumpForce => _jumpForce;
        public float MoveSpeed => _moveSpeed;
    }
}