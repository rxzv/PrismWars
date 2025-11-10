using UnityEngine;

namespace _PrismWars._Scripts.Game.GameManager {
    [CreateAssetMenu(fileName = "MapData", menuName = "Game/Map Data")]
    public class MapData : ScriptableObject {
        public string mapName;
        public Sprite preview;
        public GameObject mapPrefab;
        public Vector3[] fireSpawnPoints;
        public Vector3[] iceSpawnPoints;
        public Vector3 catFireSpawnPoints;
        public Vector3 catIceSpawnPoints;
    }
}