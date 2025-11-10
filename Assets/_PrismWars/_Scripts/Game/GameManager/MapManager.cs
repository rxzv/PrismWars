using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Game.GameManager {
    public class MapManager : NetworkBehaviour, IService {
        [SerializeField] MapData[] _availableMaps;
        
        MapData _currentMap;
        
        public MapData Data => _currentMap;
    
        public void LoadMap(string mapName) {
            _currentMap = _availableMaps.FirstOrDefault(m => m.mapName == mapName);
            if (_currentMap != null && IsServer) {
                var mapInstance = Instantiate(_currentMap.mapPrefab);
                mapInstance.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}