using Unity.Collections;
using Unity.Netcode;

namespace _PrismWars._Scripts.UI.Model {
    // Структура для сетевой синхронизации
    public struct NetworkPlayerData : INetworkSerializable, System.IEquatable<NetworkPlayerData> {
        public FixedString64Bytes playerName;
        public PlayerType playerType;
        public FixedString64Bytes spriteName;
        public float moveSpeed;
        public float jumpForce;
        public float maxHealth;
        public float meleeAttackRange;
        public float meleeDamage;
        public int enemyLayerValue;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
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

        public bool Equals(NetworkPlayerData other) {
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
}