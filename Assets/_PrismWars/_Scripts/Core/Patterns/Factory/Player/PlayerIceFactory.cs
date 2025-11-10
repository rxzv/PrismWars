using System.Collections.Generic;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Patterns.Factory {
    public class PlayerIceFactory : PlayerFactory {
        public PlayerIceFactory(Transform playerPrefab, Vector3[] spawnPoints) : base(playerPrefab, spawnPoints){}
    }
}