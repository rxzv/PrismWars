using _PrismWars._Scripts.Core.Patterns.Factory.Cat;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Patterns.Factory {
    public class CatFireFactory : CatFactory {
        public CatFireFactory(Transform playerPrefab, Transform spawnPoint) : 
            base(playerPrefab, spawnPoint){}
    }
}