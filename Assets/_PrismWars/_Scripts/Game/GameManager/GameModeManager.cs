using System.Linq;
using _PrismWars._Scripts.Core.Infrastructure.Network.Components.Cat;
using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Game.GameManager {
    public class GameModeManager : NetworkBehaviour, IService {
        [SerializeField] GameModeData[] _gameModeDatas;
        
        ServerCatSpawnService _serverCatSpawnService;
    
        GameModeData _currentGameMode;
        NetworkVariable<bool> _isSpawnedGameMode = new();
        public GameModeData Data => _currentGameMode;
        
        public void ApplyGameMode(string mapName) {
            _currentGameMode = _gameModeDatas.FirstOrDefault(m => m.name.ToString() == mapName);
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
            _serverCatSpawnService = ServiceLocator.Singleton.Get<ServerCatSpawnService>();
            _serverCatSpawnService.SpawnCatRpc(
                new NetworkCatData{catElement = PlayerElement.Fire}, 
                NetworkManager.Singleton.LocalClientId);
            _serverCatSpawnService.SpawnCatRpc(
                new NetworkCatData{catElement = PlayerElement.Ice}, 
                NetworkManager.Singleton.LocalClientId);
        }
    }
}