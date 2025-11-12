using UnityEngine;

namespace _PrismWars._Scripts.Core.Patterns.Factory.Player {
    public class PlayerFireFactory : PlayerFactory {
        public PlayerFireFactory(Transform playerPrefab, Vector3[] spawnPoints) : base(playerPrefab, spawnPoints){}
    }
}