using System;
using System.Linq;
using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Services;
using _PrismWars._Scripts.Core.Infrastructure.Network.Components.Cat;
using _PrismWars._Scripts.Game.GameManagers.Datas;
using _PrismWars._Scripts.Game.Player.Model;
using _PrismWars._Scripts.Game.Services.Server;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Game.GameManagers.Managers {
    public class GameModeManager : NetworkBehaviour, IService {
        [SerializeField] GameModeData[] _gameModeDatas;

        const string HILL_CAPTURE_COMPONENT_PATH = "NetworkServicePrefabs/HillCaptureComponent";
        const string BOMB_CART_PATH = "NetworkServicePrefabs/BombCart";
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
            if(_currentGameMode == null || _isSpawnedGameMode.Value) return;
            _isSpawnedGameMode.Value = true;
            switch (_currentGameMode.modeName) {
                case GameMode.Deathmatch:
                    break;
                case GameMode.KingOfTheHill:
                    SpawnObject(HILL_CAPTURE_COMPONENT_PATH);
                    break;
                case GameMode.CaptureTheCat:
                    SpawnServerCatSpawnService();
                    break;
                case GameMode.BombLoad:
                    SpawnObject(BOMB_CART_PATH);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        void SpawnObject(string path) {
            if(!IsServer) return;
            var prefab = Resources.Load<GameObject>(path);
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