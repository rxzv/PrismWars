using System.Collections.Generic;
using _PrismWars._Scripts.Core.Infrastructure.Network.Components;
using _PrismWars._Scripts.Core.Infrastructure.Network.Components.Cat;
using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Patterns.Factory.Cat {
    public class CatFactory : Factory{

        public CatFactory(Transform prefab, List<Transform> spawnPoints) 
            : base(prefab, spawnPoints) { }

        public override NetworkObject Spawn(INetworkData data, ulong senderClientId) {
            int spawnId = (int)senderClientId;
            var playerInstance = Object.Instantiate(_prefab, _spawnPoints[spawnId].position, Quaternion.identity);
            var networkCat = playerInstance.GetComponent<CatController>();
            var networkObjectReference = playerInstance.GetComponent<NetworkObject>();
            playerInstance.GetComponent<NetworkObject>().SpawnWithOwnership(senderClientId);
            networkCat.Initialize((NetworkCatData)data);
            return networkObjectReference;
        }

    }
}