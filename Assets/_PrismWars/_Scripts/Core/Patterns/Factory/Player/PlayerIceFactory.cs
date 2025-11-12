using UnityEngine;

namespace _PrismWars._Scripts.Core.Patterns.Factory.Player {
    public class PlayerIceFactory : PlayerFactory {
        public PlayerIceFactory(Transform playerPrefab, Vector3[] spawnPoints) : base(playerPrefab, spawnPoints){}
    }
}