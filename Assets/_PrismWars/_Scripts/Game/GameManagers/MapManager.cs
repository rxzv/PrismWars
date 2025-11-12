using System.Linq;
using _PrismWars._Scripts.Game.GameManagers.Datas;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Game.GameManagers {
    public class MapManager : NetworkBehaviour, IService {
        [SerializeField] MapData[] _availableMaps;
        
        MapData _currentMap;
        NetworkVariable<bool> _isSpawnedMap = new();
        
        public MapData Data => _currentMap;
    
        public void LoadMap(string mapName) {
            _currentMap = _availableMaps.FirstOrDefault(m => m.mapName == mapName);
            if (_currentMap != null && IsServer && !_isSpawnedMap.Value) {
                _isSpawnedMap.Value = true;
                var mapInstance = Instantiate(_currentMap.mapPrefab);
                mapInstance.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}