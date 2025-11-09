using System.Collections.Generic;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Patterns.Factory {
    public class PlayerFireFactory : PlayerFactory {
        public PlayerFireFactory(Transform playerPrefab, List<Transform> spawnPoints) : base(playerPrefab, spawnPoints){}
    }
}