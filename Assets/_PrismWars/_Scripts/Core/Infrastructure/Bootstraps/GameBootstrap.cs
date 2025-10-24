using System;
using System.Collections.Generic;
using _PrismWars._Scripts.Components.Projectile;
using _PrismWars._Scripts.Core.Patterns.Factory;
using _PrismWars._Scripts.Game.GameManager;
using _PrismWars._Scripts.UI;
using Unity.Cinemachine;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Infrastructure.Bootstraps {
    public class GameBootstrap : MonoBehaviour {
        [Header("Mono Services")] 
        [SerializeField] CameraSpawnService _cameraSpawnService;
        
        [Header("Network Services")] 
        [SerializeField] NetworkUIManager _networkUIManager;
        [SerializeField] NetworkGameManager _networkGameManager;
        [SerializeField] NetworkPlayerSpawnService _networkPlayerSpawnService;
        
        [Header("Scene Components")]
        [SerializeField] List<Transform> _fireSpawnPoints;
        [SerializeField] List<Transform> _iceSpawnPoints;
        
        ProjectileServerService _projectileServerService;
        
        List<IDisposable> _disposables = new();

        void Start() {
            RegisterServices();
            InitializeServices();
            AddDisposables();
        }

        void RegisterServices() {
            ServiceLocator.Singleton.Register(_networkUIManager);
            ServiceLocator.Singleton.Register(_networkGameManager);
            ServiceLocator.Singleton.Register(_networkPlayerSpawnService);
            ServiceLocator.Singleton.Register(_cameraSpawnService);
            
            _projectileServerService = new ProjectileServerService();
            ServiceLocator.Singleton.Register(_projectileServerService);
            
            Debug.Log("GameScene Services registered");
        }
        void InitializeServices() {
            var playerPrefab = Resources.Load<Transform>("Prefabs/Player");
            var playerFireFactory = new PlayerFireFactory(playerPrefab, _fireSpawnPoints);
            var playerIceFactory = new PlayerIceFactory(playerPrefab, _iceSpawnPoints);
            var cameraPrefab = Resources.Load<CinemachineCamera>("Prefabs/CinemachineCamera");

            _networkPlayerSpawnService.Initialize(playerFireFactory, playerIceFactory);
            _networkUIManager.Initialize();
            _projectileServerService.Initialize();
            _cameraSpawnService.Initialize(cameraPrefab);
            
            _networkGameManager.Initialize();
            
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