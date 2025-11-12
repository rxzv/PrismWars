using Unity.Netcode;

namespace _PrismWars._Scripts.Game.Player.Model {
    [System.Serializable]
    public class JumpingData : INetworkSerializable {
        public int tick;
        public float positionY;
        public bool isJumping;
        public bool isGrounded;
        public bool isRespawning;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref positionY);
            serializer.SerializeValue(ref isJumping);
            serializer.SerializeValue(ref isGrounded);
            serializer.SerializeValue(ref isRespawning);
        }
    }
}