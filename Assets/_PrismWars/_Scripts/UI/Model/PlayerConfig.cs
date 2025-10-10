using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.UI.Model
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Configs/Player Config")]
    public class PlayerConfig : ScriptableObject
    {
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
        public NetworkPlayerConfig ToNetworkConfig()
        {
            return new NetworkPlayerConfig
            {
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
        public void FromNetworkConfig(NetworkPlayerConfig networkConfig)
        {
            playerName = networkConfig.playerName.ToString();
            playerType = networkConfig.playerType;
            spriteName = networkConfig.spriteName.ToString();
            moveSpeed = networkConfig.moveSpeed;
            jumpForce = networkConfig.jumpForce;
            maxHealth = networkConfig.maxHealth;
            meleeAttackRange = networkConfig.meleeAttackRange;
            meleeDamage = networkConfig.meleeDamage;
            enemyLayer = new LayerMask { value = networkConfig.enemyLayerValue };
            
            // Загрузка спрайта по имени
            if (!string.IsNullOrEmpty(spriteName))
                sprite = Resources.Load<Sprite>(spriteName);
        }
    }
    // Структура для сетевой синхронизации
    public struct NetworkPlayerConfig : INetworkSerializable, System.IEquatable<NetworkPlayerConfig>
    {
        public FixedString64Bytes playerName;
        public PlayerType playerType;
        public FixedString64Bytes spriteName;
        public float moveSpeed;
        public float jumpForce;
        public float maxHealth;
        public float meleeAttackRange;
        public float meleeDamage;
        public int enemyLayerValue;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref playerName);
            serializer.SerializeValue(ref playerType);
            serializer.SerializeValue(ref spriteName);
            serializer.SerializeValue(ref moveSpeed);
            serializer.SerializeValue(ref jumpForce);
            serializer.SerializeValue(ref maxHealth);
            serializer.SerializeValue(ref meleeAttackRange);
            serializer.SerializeValue(ref meleeDamage);
            serializer.SerializeValue(ref enemyLayerValue);
        }

        public bool Equals(NetworkPlayerConfig other)
        {
            return playerName.Equals(other.playerName) &&
                   playerType == other.playerType &&
                   spriteName.Equals(other.spriteName) &&
                   moveSpeed.Equals(other.moveSpeed) &&
                   jumpForce.Equals(other.jumpForce) &&
                   maxHealth.Equals(other.maxHealth) &&
                   meleeAttackRange.Equals(other.meleeAttackRange) &&
                   meleeDamage.Equals(other.meleeDamage) &&
                   enemyLayerValue == other.enemyLayerValue;
        }
    }

    public enum PlayerType {
        Fire,
        Ice
    }
}