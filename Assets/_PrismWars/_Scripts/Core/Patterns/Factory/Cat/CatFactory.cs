using System.Collections.Generic;
using _PrismWars._Scripts.Core.Infrastructure.Network.Components;
using _PrismWars._Scripts.Core.Infrastructure.Network.Components.Cat;
using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Patterns.Factory.Cat {
    public class CatFactory : Factory{

        protected CatFactory(Transform prefab, Transform spawnPoint) 
            : base(prefab, spawnPoint) { }

        public override NetworkObject Spawn(INetworkData data, ulong senderClientId) {
            var playerInstance = Object.Instantiate(_prefab, _spawnPoint.position, Quaternion.identity);
            var networkCat = playerInstance.GetComponent<CatController>();
            var networkObjectReference = playerInstance.GetComponent<NetworkObject>();
            playerInstance.GetComponent<NetworkObject>().SpawnWithOwnership(senderClientId);
            networkCat.Initialize((NetworkCatData)data);
            return networkObjectReference;
        }
    }
}