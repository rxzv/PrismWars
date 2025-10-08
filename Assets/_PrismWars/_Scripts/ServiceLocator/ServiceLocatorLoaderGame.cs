using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _PrismWars._Scripts;
using _PrismWars._Scripts.Components.Projectile;
using _PrismWars._Scripts.Player;
using _PrismWars._Scripts.Systems;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class ServiceLocatorLoaderGame : MonoBehaviour {

    [Header("Services")]
    [SerializeField] CameraSpawnService _cameraSpawnService;
    [SerializeField] InputService _inputService;
    [SerializeField] PlayerSpawnService _playerSpawnService;    
    [SerializeField] CursorService _cursorService;
    [SerializeField] ProjectileFactory _projectileFactory;
    [SerializeField] AttackRangeController _attackRangeController;
    
    ProjectileService _projectileService;
    Transform _playerPrefab;
    CinemachineCamera _cameraPrefab;
    
    List<IDisposable> _disposables = new();

    void Awake() {
        RegisterServices();
        Init();
        AddDisposables();
    }
    
    void RegisterServices() {
        ServiceLocator.Initialize();
        
        _cameraPrefab = Resources.Load<CinemachineCamera>("Prefabs/CinemachineCamera");
        _playerPrefab = Resources.Load<Transform>("Prefabs/Player");
        
        _projectileService = new ProjectileService();
        
        ServiceLocator.Current.Register(_playerSpawnService);
        ServiceLocator.Current.Register(_inputService);
        ServiceLocator.Current.Register(_cameraSpawnService);
        ServiceLocator.Current.Register(_cursorService);
        ServiceLocator.Current.Register(_projectileFactory);
        ServiceLocator.Current.Register(_projectileService);
        ServiceLocator.Current.Register(_attackRangeController);
    }

    async void Init() {
        await WaitForInstanceAsync();
        
        _playerSpawnService.Initialize(_playerPrefab);
        _inputService.Initialize();
        _cameraSpawnService.Initialize(_cameraPrefab);
        _cursorService.Initialize();
        _projectileService.Initialize();
        _attackRangeController.Initialize();
        
        Debug.Log("Service initialized");
    }

    void AddDisposables() {
        _disposables.Add(_inputService);
        _disposables.Add(_playerSpawnService);
        _disposables.Add(_cameraSpawnService);
        _disposables.Add(_projectileService);
    }
    private async Task WaitForInstanceAsync() {
        while (NetworkManager.Singleton == null) {
            await Task.Yield();
        }
    }

    void OnDestroy() {
        foreach (var disposable in _disposables) {
            disposable.Dispose();
        }
    }
}