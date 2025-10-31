using Unity.Netcode;

namespace _PrismWars._Scripts.UI.Model {
    [System.Serializable]
    public class MovementData : INetworkSerializable {
        public int tick;
        public float movementDirection;
        public float positionX;
        public bool isRespawning;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref movementDirection);
            serializer.SerializeValue(ref positionX);
            serializer.SerializeValue(ref isRespawning);
        }
    }
}