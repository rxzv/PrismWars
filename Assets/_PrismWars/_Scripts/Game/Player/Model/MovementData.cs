using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.UI.Model {
    [System.Serializable]
    public class MovementData : INetworkSerializable {
        public int tick;
        public Vector2 movementDirection;
        public Vector2 position;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref movementDirection);
            serializer.SerializeValue(ref position);
        }
    }
}