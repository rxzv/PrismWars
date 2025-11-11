using System.Linq;
using _PrismWars._Scripts.Core.Infrastructure.Network.Components.Cat;
using _PrismWars._Scripts.Core.Patterns.Factory;
using _PrismWars._Scripts.Game.Services;
using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Game.GameManager {
    public class GameModeManager : NetworkBehaviour, IService {
        [SerializeField] GameModeData[] _gameModeDatas;
    
        GameModeData _currentGameMode;
        NetworkVariable<bool> _isSpawnedGameMode = new();
        public GameModeData Data => _currentGameMode;
        
        MapManager _mapManager;

        ServerCatSpawnService _catSpawnService;
        
        public void ApplyGameMode(string mapName) {
            _currentGameMode = _gameModeDatas.FirstOrDefault(m => m.name.ToString() == mapName);
        }

        public void SpawnGameMode() {
            if(!IsServer) return;
            if (_currentGameMode != null && !_isSpawnedGameMode.Value) {
                _isSpawnedGameMode.Value = true;
                switch (_currentGameMode.modeName) {
                    case GameMode.Deathmatch:
                        //
                        break;
                    case GameMode.KingOfTheHill:
                        SpawnHillCaptureComponent();
                        break;
                    case GameMode.CaptureTheCat:
                        SpawnServerCatSpawnService();
                        break;
                    case GameMode.BombLoad:
                        //
                        break;
                }
            }
        }

        void SpawnHillCaptureComponent() {
            if(!IsServer) return;
            var prefab = Resources.Load<GameObject>("NetworkServicePrefabs/HillCaptureComponent");
            var instance = Instantiate(prefab);
            instance.GetComponent<NetworkObject>().Spawn();
        }

        void SpawnServerCatSpawnService() {
            if(!IsServer) return;
            _catSpawnService = ServiceLocator.Singleton.Get<ServerCatSpawnService>();
            _catSpawnService.SpawnCatRpc(
                new NetworkCatData{catElement = PlayerElement.Fire}, 
                NetworkManager.Singleton.LocalClientId);
            _catSpawnService.SpawnCatRpc(
                new NetworkCatData{catElement = PlayerElement.Ice}, 
                NetworkManager.Singleton.LocalClientId);
        }
    }
}