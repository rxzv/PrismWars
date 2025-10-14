using UnityEngine;

namespace _PrismWars._Scripts.UI.Model {
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Configs/Player Config")]
    public class PlayerConfig : ScriptableObject {
        public string playerName;
        public PlayerType playerType = PlayerType.Fire;
        public string spriteName;
        public Sprite sprite;
        public float moveSpeed = 10f;
        public float jumpForce = 10f;
        public float maxHealth = 100f;
        public float meleeAttackRange = 1;
        public float meleeDamage = 1;
        public LayerMask enemyLayer;

        // Конвертация в сетевую структуру
        public NetworkPlayerData ToNetworkConfig() {
            return new NetworkPlayerData {
                playerName = playerName,
                playerType = playerType,
                spriteName = spriteName,
                moveSpeed = moveSpeed,
                jumpForce = jumpForce,
                maxHealth = maxHealth,
                meleeAttackRange = meleeAttackRange,
                meleeDamage = meleeDamage,
                enemyLayerValue = enemyLayer.value
            };
        }

        // Восстановление из сетевой структуры
        public void FromNetworkConfig(NetworkPlayerData networkData) {
            playerName = networkData.playerName.ToString();
            playerType = networkData.playerType;
            spriteName = networkData.spriteName.ToString();
            moveSpeed = networkData.moveSpeed;
            jumpForce = networkData.jumpForce;
            maxHealth = networkData.maxHealth;
            meleeAttackRange = networkData.meleeAttackRange;
            meleeDamage = networkData.meleeDamage;
            enemyLayer = new LayerMask { value = networkData.enemyLayerValue };
            
            // Загрузка спрайта по имени
            if (!string.IsNullOrEmpty(spriteName))
                sprite = Resources.Load<Sprite>($"Sprites/{spriteName}");
        }
    }

    public enum PlayerType {
        Fire,
        Ice
    }
}