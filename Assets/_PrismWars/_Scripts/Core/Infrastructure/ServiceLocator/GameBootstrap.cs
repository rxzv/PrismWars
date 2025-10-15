using System;
using System.Collections.Generic;
using _PrismWars._Scripts;
using _PrismWars._Scripts.Components.Projectile;
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
    [SerializeField] AttackRangeController _attackRangeController;
    [SerializeField] CharacterSelectionManager _characterSelectionManager;
    
    ProjectileService _projectileService;
    Transform _playerPrefab;
    CinemachineCamera _cameraPrefab;
    
    List<IDisposable> _disposables = new();
    public override void OnNetworkSpawn() {
        RegisterServices();
        Init();
        AddDisposables();
    }
    
    void RegisterServices() {
        _cameraPrefab = Resources.Load<CinemachineCamera>("Prefabs/CinemachineCamera");
        _playerPrefab = Resources.Load<Transform>("Prefabs/Player");
        
        _projectileService = new ProjectileService();
        
        ServiceLocator.Singleton.Register(_playerSpawnService);
        ServiceLocator.Singleton.Register(_inputService);
        ServiceLocator.Singleton.Register(_cameraSpawnService);
        ServiceLocator.Singleton.Register(_cursorService);
        ServiceLocator.Singleton.Register(_projectileFactory);
        ServiceLocator.Singleton.Register(_projectileService);
        ServiceLocator.Singleton.Register(_attackRangeController);
        ServiceLocator.Singleton.Register(_characterSelectionManager);
    }

    void Init() {
        _playerSpawnService.Initialize(_playerPrefab);
        _inputService.Initialize();
        _cameraSpawnService.Initialize(_cameraPrefab);
        _cursorService.Initialize();
        _projectileService.Initialize();
        _attackRangeController.Initialize();
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