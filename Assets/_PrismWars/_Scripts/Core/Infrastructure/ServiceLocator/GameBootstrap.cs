using System;
using System.Collections.Generic;
using _PrismWars._Scripts;
using _PrismWars._Scripts.Components.Projectile;
using _PrismWars._Scripts.Core.Patterns.Factory;
using _PrismWars._Scripts.Player;
using _PrismWars._Scripts.Systems;
using _PrismWars._Scripts.UI;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class GameBootstrap : NetworkBehaviour {

    [Header("Services")]
    [SerializeField] CameraSpawnService _cameraSpawnService;
    [SerializeField] InputService _inputService;
    [SerializeField] PlayerSpawnService _playerSpawnService;    
    [SerializeField] CursorService _cursorService;
    [SerializeField] ProjectileFactory _projectileFactory;
    [SerializeField] CharacterSelectionManager _characterSelectionManager;
    
    [Header("SceneComponents")]
    [SerializeField] List<Transform> _fireSpawnPoints;
    [SerializeField] List<Transform> _iceSpawnPoints;
    
    PlayerFireFactory _playerFireFactory;
    PlayerIceFactory _playerIceFactory;
    
    ProjectileService _projectileService;
    Transform _playerPrefab;
    CinemachineCamera _cameraPrefab;
    Projectile _projectilePrefab;
    
    List<IDisposable> _disposables = new();
    public override void OnNetworkSpawn() {
        RegisterServices();
        Init();
        AddDisposables();
    }
    
    void RegisterServices() {
        _cameraPrefab = Resources.Load<CinemachineCamera>("Prefabs/CinemachineCamera");
        _playerPrefab = Resources.Load<Transform>("Prefabs/Player");
        _projectilePrefab = Resources.Load<Projectile>("Prefabs/Projectile");
        
        _projectileService = new ProjectileService();
        
        _playerFireFactory = new PlayerFireFactory(_playerPrefab, _fireSpawnPoints);
        _playerIceFactory = new PlayerIceFactory(_playerPrefab, _iceSpawnPoints);
        
        ServiceLocator.Singleton.Register(_playerSpawnService);
        ServiceLocator.Singleton.Register(_inputService);
        ServiceLocator.Singleton.Register(_cameraSpawnService);
        ServiceLocator.Singleton.Register(_cursorService);
        ServiceLocator.Singleton.Register(_projectileFactory);
        ServiceLocator.Singleton.Register(_projectileService);
        ServiceLocator.Singleton.Register(_characterSelectionManager);
    }

    void Init() {
        _playerSpawnService.Initialize(_playerFireFactory, _playerIceFactory);
        _inputService.Initialize();
        _cameraSpawnService.Initialize(_cameraPrefab);
        _cursorService.Initialize();
        _projectileFactory.Initialize(_projectilePrefab);
        _projectileService.Initialize();
        _characterSelectionManager.Initialize();
            
        Debug.Log("Service initialized");
    }

    void AddDisposables() {
        _disposables.Add(_inputService);
        _disposables.Add(_cameraSpawnService);
        _disposables.Add(_projectileService);
    }

    void OnDestroy() {
        foreach (var disposable in _disposables) {
            disposable.Dispose();
        }
    }
}