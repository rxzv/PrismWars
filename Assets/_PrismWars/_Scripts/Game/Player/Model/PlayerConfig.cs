using UnityEngine;

namespace _PrismWars._Scripts.Game.Player.Model {
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Configs/Player Config")]
    public class PlayerConfig : ScriptableObject {
        public int configId;
        public string playerName;
        public PlayerElement playerElement = PlayerElement.Fire;
        public string spriteName;
        public Sprite sprite;
        public float moveSpeed = 10f;
        public float jumpForce = 10f;
        public float maxHealth = 100f;
        public float meleeAttackRange = 1;
        public float meleeDamage = 1;
        public LayerMask enemyLayer;
        public int maxBulletCount = 4;

        public NetworkPlayerData ToNetworkConfig() {
            return new NetworkPlayerData {
                configId = configId,
                playerName = playerName,
                playerElement = playerElement,
                spriteName = spriteName,
                moveSpeed = moveSpeed,
                jumpForce = jumpForce,
                maxHealth = maxHealth,
                meleeAttackRange = meleeAttackRange,
                meleeDamage = meleeDamage,
                enemyLayerValue = enemyLayer.value,
                maxBulletCount = maxBulletCount,
            };
        }

        public void FromNetworkConfig(NetworkPlayerData networkData) {
            configId = networkData.configId;
            playerName = networkData.playerName.ToString();
            playerElement = networkData.playerElement;
            spriteName = networkData.spriteName.ToString();
            moveSpeed = networkData.moveSpeed;
            jumpForce = networkData.jumpForce;
            maxHealth = networkData.maxHealth;
            meleeAttackRange = networkData.meleeAttackRange;
            meleeDamage = networkData.meleeDamage;
            maxBulletCount = networkData.maxBulletCount;
            enemyLayer = new LayerMask { value = networkData.enemyLayerValue };
            
            if (!string.IsNullOrEmpty(spriteName))
                sprite = Resources.Load<Sprite>($"Sprites/{spriteName}");
        }
    }

    public enum PlayerElement{
        None,
        Fire,
        Ice
    }
}