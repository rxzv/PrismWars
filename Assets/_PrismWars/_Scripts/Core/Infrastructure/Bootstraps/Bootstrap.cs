using System;
using System.Collections.Generic;
using _PrismWars._Scripts.Core.Infrastructure.Network.Components.ProjectileComponent;
using _PrismWars._Scripts.Core.Infrastructure.Network.Components.Shard;
using _PrismWars._Scripts.Core.Patterns.Factory.Cat;
using _PrismWars._Scripts.Core.Patterns.Factory.ProjectileFactory;
using _PrismWars._Scripts.Core.Patterns.Factory.ShardFactory;
using _PrismWars._Scripts.Game.GameManagers.Datas;
using _PrismWars._Scripts.Game.GameManagers.Managers;
using _PrismWars._Scripts.Game.Services;
using _PrismWars._Scripts.Game.Services.Mono;
using _PrismWars._Scripts.Game.Services.Server;
using _PrismWars._Scripts.UI.CharacterSelection;
using _PrismWars._Scripts.Utils;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _PrismWars._Scripts.Core.Infrastructure.Bootstraps {
    public class Bootstrap : MonoBehaviour {
        public const string GAME_SCENE_NAME = "Game";
    
        [Header("Mono Services")] 
        [SerializeField] InputService _inputService;
    
        [Header("Network Services")] 
        [SerializeField] NetworkTimer networkTimer;
        [SerializeField] NetworkCharacterSelectionManager _networkCharacterSelectionManager;
        [SerializeField] ProjectileFactory _projectileFactory;
        [SerializeField] NetworkScoreService  _networkScoreService;
        [SerializeField] ShardFactory _shardFactory;
        [SerializeField] NetworkChangeScene _networkChangeScene;
        [SerializeField] MapManager _mapManager;
        [SerializeField] GameModeManager _gameModeManager;
        [SerializeField] ServerCatSpawnService _catSpawnService;
        [SerializeField] CatRespawnService _catRespawnService;
        [SerializeField] AwaitingInitializationService _awaitingInitializationService;
    
        List<IDisposable> _disposables = new();
    
        void Start() {
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoaded;
            RegisterServices();
            InitializeServices();
        }
        void OnSceneLoaded(ulong clientId, string sceneName, LoadSceneMode loadSceneMode) {
            if (sceneName == GAME_SCENE_NAME) {
                ApplyGameSettings();
            }
        }
        void ApplyGameSettings() {
            var sessionManager = GameLobbyManager.Instance;
            var gameMode = sessionManager.GameMode;
            var mapData = sessionManager.Map;
        
            _mapManager.LoadMap(mapData);
            _gameModeManager.ApplyGameMode(gameMode);
        
            if(_gameModeManager.Data.modeName == GameMode.CaptureTheCat) {
                CatInitialize();
            }

            _gameModeManager.SpawnGameMode();
        }
        void CatInitialize() {
            var catPrefab = Resources.Load<Transform>("Prefabs/Cat");
            var catFireFactory = new CatFireFactory(catPrefab, _mapManager.Data.catFireSpawnPoint);
            var catIceFactory = new CatIceFactory(catPrefab, _mapManager.Data.catIceSpawnPoint);

            _catSpawnService.Initialize(catFireFactory, catIceFactory);
            _catRespawnService.Initialize(_mapManager.Data.catFireSpawnPoint, _mapManager.Data.catIceSpawnPoint);
        }
    
        void RegisterServices() {
            ServiceLocator.Singleton.Register(_inputService);
            ServiceLocator.Singleton.Register(_networkCharacterSelectionManager);
            ServiceLocator.Singleton.Register(networkTimer);
            ServiceLocator.Singleton.Register(_projectileFactory);
            ServiceLocator.Singleton.Register(_shardFactory);
            ServiceLocator.Singleton.Register(_networkScoreService);
            ServiceLocator.Singleton.Register(_networkChangeScene);
            ServiceLocator.Singleton.Register(_gameModeManager);
            ServiceLocator.Singleton.Register(_mapManager);
            ServiceLocator.Singleton.Register(_catSpawnService);
            ServiceLocator.Singleton.Register(_catRespawnService);
            ServiceLocator.Singleton.Register(_awaitingInitializationService);
        
            DontDestroyOnLoad(this);
            DontDestroyOnLoad(_inputService);
            DontDestroyOnLoad(_projectileFactory);
            DontDestroyOnLoad(_networkCharacterSelectionManager);
            DontDestroyOnLoad(networkTimer);
            DontDestroyOnLoad(_networkScoreService);
            DontDestroyOnLoad(_shardFactory);
            DontDestroyOnLoad(_networkChangeScene);
            DontDestroyOnLoad(_gameModeManager);
            DontDestroyOnLoad(_mapManager);
            DontDestroyOnLoad(_catSpawnService);
            DontDestroyOnLoad(_catRespawnService);
            DontDestroyOnLoad(_awaitingInitializationService);
        
            Debug.Log("BootstrapScene Services registered");
        }
        void InitializeServices() {
            var projectilePrefab = Resources.Load<Projectile>("Prefabs/Projectile");
            var shardPrefab = Resources.Load<Shard>("Prefabs/Shard");
        
            _awaitingInitializationService.Initialize(GAME_SCENE_NAME);
            _inputService.Initialize();
            _projectileFactory.Initialize(projectilePrefab);
            _shardFactory.Initialize(shardPrefab);
        
            _awaitingInitializationService.ClientBootstrapInitialized();
            Debug.Log("BootstrapScene Services initialize");
        }

        void OnDestroy() {
            foreach (var disposable in _disposables) {
                disposable.Dispose();
            }
        }

    }
}