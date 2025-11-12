using UnityEngine;

namespace _PrismWars._Scripts.Core.Patterns.Factory.Cat {
    public class CatFireFactory : CatFactory {
        public CatFireFactory(Transform playerPrefab, Vector3 spawnPoint) : 
            base(playerPrefab, spawnPoint){}
    }
}