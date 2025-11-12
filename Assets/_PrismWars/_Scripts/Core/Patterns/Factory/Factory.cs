using _PrismWars._Scripts.Core.Infrastructure.Interfaces;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Patterns.Factory {
    public abstract class Factory {
        protected Transform _prefab;
        protected Vector3[] _spawnPoints;
        protected Vector3 _spawnPoint;
        
        protected Factory(Transform prefab, Vector3 spawnPoints) {
            _prefab =  prefab;
            _spawnPoint = spawnPoints;
        }
        protected Factory(Transform prefab, Vector3[] spawnPoints) {
            _prefab =  prefab;
            _spawnPoints = spawnPoints;
        }

        public abstract NetworkObject Spawn(INetworkData data, ulong senderClientId);
    }
}