using _PrismWars._Scripts.Core.Patterns.Factory.Cat;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Patterns.Factory {
    public class CatIceFactory : CatFactory {
        public CatIceFactory(Transform playerPrefab, Vector3 spawnPoint) : 
            base(playerPrefab, spawnPoint){}
    }
}