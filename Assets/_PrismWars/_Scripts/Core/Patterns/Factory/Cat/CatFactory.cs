using _PrismWars._Scripts.Core.Infrastructure.Network.Components.Cat;
using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Patterns.Factory.Cat {
    public class CatFactory : Factory{

        protected CatFactory(Transform prefab, Vector3 spawnPoint) 
            : base(prefab, spawnPoint) { }

        public override NetworkObject Spawn(INetworkData data, ulong senderClientId) {
            var playerInstance = Object.Instantiate(_prefab, _spawnPoint, Quaternion.identity);
           
            playerInstance.TryGetComponent(out CatController networkCat);
            playerInstance.TryGetComponent(out NetworkObject networkObject);
            if(networkCat is null || networkObject is null) return null;
            networkObject.SpawnWithOwnership(senderClientId);
            var ncd = (NetworkCatData)data;
            networkCat.Initialize(ncd);
            return networkObject;
        }
    }
}