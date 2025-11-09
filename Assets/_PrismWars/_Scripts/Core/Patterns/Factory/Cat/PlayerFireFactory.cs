using System.Collections.Generic;
using _PrismWars._Scripts.Core.Patterns.Factory.Cat;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Patterns.Factory {
    public class CatFireFactory : CatFactory {
        public CatFireFactory(Transform playerPrefab, List<Transform> spawnPoints) : 
            base(playerPrefab, spawnPoints){}
    }
}