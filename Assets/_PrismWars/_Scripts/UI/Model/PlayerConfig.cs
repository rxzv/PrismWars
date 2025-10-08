using UnityEngine;
using UnityEngine.Serialization;

namespace _PrismWars._Scripts.UI.Model
{ 
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Configs/Player Config")]
    public class PlayerConfig : ScriptableObject {
        public string playerName;
        public PlayerType playerType = PlayerType.Fire;
        public Sprite sprite;
        public float moveSpeed = 10f;
        public float jumpForce = 10f;
        public float maxHealth = 100f;
        public float meleeAttackRange = 1;
        public float meleeDamage = 1;
        public LayerMask enemyLayer;
    }
}