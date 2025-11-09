using System;
using System.Collections.Generic;
using _PrismWars._Scripts.Components.Projectile;
using _PrismWars._Scripts.Core.Patterns.Factory;
using _PrismWars._Scripts.Game.Services;
using _PrismWars._Scripts.UI;
using Unity.Cinemachine;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Infrastructure.Bootstraps {
    public class GameBootstrap : MonoBehaviour {
        [Header("Mono Services")] 
        [SerializeField] CameraSpawnService _cameraSpawnService;
        
        [Header("Network Services")] 
        [SerializeField] NetworkUIManager _networkUIManager;
        [SerializeField] GameOverUIService _gameOverUIService;
        [SerializeField] NetworkPlayerSpawnService _networkPlayerSpawnService;
        [SerializeField] NetworkSpawner _networkSpawner;
        [SerializeField] PlayerRespawnService _playerRespawnService;
        [SerializeField] ServerCatSpawnService _serverCatSpawnService;
        [SerializeField] CatRespawnService _catRespawnService;
        
        [Header("Scene Components")]
        [SerializeField] List<Transform> _fireSpawnPoints;
        [SerializeField] List<Transform> _iceSpawnPoints;
        [SerializeField] Transform _catFireSpawnPoint;
        [SerializeField] Transform _catIceSpawnPoint;
        
        ProjectileServerService _projectileServerService;
        ShardServerService _shardServerService;
        
        List<IDisposable> _disposables = new();

        void Start() {
            RegisterServices();
            InitializeServices();
            AddDisposables();
        }

        void RegisterServices() {
            ServiceLocator.Singleton.Register(_networkUIManager);
            ServiceLocator.Singleton.Register(_networkPlayerSpawnService);
            ServiceLocator.Singleton.Register(_cameraSpawnService);
            ServiceLocator.Singleton.Register(_networkSpawner);
            ServiceLocator.Singleton.Register(_playerRespawnService);
            ServiceLocator.Singleton.Register(_gameOverUIService);
            ServiceLocator.Singleton.Register(_serverCatSpawnService);
            ServiceLocator.Singleton.Register(_catRespawnService);
            
            _projectileServerService = new ProjectileServerService();
            ServiceLocator.Singleton.Register(_projectileServerService);
            
            _shardServerService = new ShardServerService();
            ServiceLocator.Singleton.Register(_shardServerService);
            
            Debug.Log("GameScene Services registered");
        }
        void InitializeServices() {
            var playerPrefab = Resources.Load<Transform>("Prefabs/Player");
            var playerFireFactory = new PlayerFireFactory(playerPrefab, _fireSpawnPoints);
            var playerIceFactory = new PlayerIceFactory(playerPrefab, _iceSpawnPoints);
            
            var cameraPrefab = Resources.Load<CinemachineCamera>("Prefabs/CinemachineCamera");

            var catPrefab = Resources.Load<Transform>("Prefabs/Cat");
            var catFireFactory = new CatFireFactory(catPrefab, _catFireSpawnPoint);
            var catIceFactory = new CatIceFactory(catPrefab, _catIceSpawnPoint);
            
            _networkPlayerSpawnService.Initialize(playerFireFactory, playerIceFactory);
            _networkUIManager.Initialize();
            _projectileServerService.Initialize();
            _shardServerService.Initialize();
            _cameraSpawnService.Initialize(cameraPrefab);
            _playerRespawnService.Initialize(_fireSpawnPoints, _iceSpawnPoints);
            _gameOverUIService.Initialize();
            _serverCatSpawnService.Initialize(catFireFactory, catIceFactory);
            _catRespawnService.Initialize(_catFireSpawnPoint, _catIceSpawnPoint);
            
            _networkSpawner.ClientInitialized();
            Debug.Log("GameScene Services initialized");
        }

        void AddDisposables() {
            var inputService = ServiceLocator.Singleton.Get<InputService>();
            _disposables.Add(_projectileServerService);
            _disposables.Add(_cameraSpawnService);
            _disposables.Add(inputService);
        }
        
        void OnDestroy() {
            foreach (var disposable in _disposables) {
                disposable.Dispose();
            }
        }
    }
}