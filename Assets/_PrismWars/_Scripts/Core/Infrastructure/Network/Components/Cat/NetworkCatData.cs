using _PrismWars._Scripts.Core.Infrastructure.Interfaces;
using _PrismWars._Scripts.Game.Player.Model;
using Unity.Netcode;

namespace _PrismWars._Scripts.Core.Infrastructure.Network.Components.Cat {
    public struct NetworkCatData : INetworkSerializable, System.IEquatable<NetworkCatData>,INetworkData {
        public PlayerElement catElement;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref catElement);
        }

        public bool Equals(NetworkCatData other) {
            return catElement == other.catElement;
        }

    }
}