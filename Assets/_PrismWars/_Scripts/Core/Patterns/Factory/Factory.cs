using System.Collections.Generic;
using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Patterns.Factory {
    public abstract class Factory {
        protected Transform _prefab;
        protected List<Transform> _spawnPoints;
        
        public Factory(Transform prefab, List<Transform> spawnPoints) {
            _prefab =  prefab;
            _spawnPoints = spawnPoints;
        }

        public abstract NetworkObject Spawn(INetworkData data, ulong senderClientId);
    }
}