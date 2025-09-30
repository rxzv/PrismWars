using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _PrismWars._Scripts;
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
    [SerializeField] FlyweightFactory _flyweightFactory;
    
    [Header("Prefabs")]
    [SerializeField] Transform _playerPrefab;
    [SerializeField] CinemachineCamera _cameraPrefab;
    
    [Header("Configs")]
    [SerializeField] PlayerConfig _playerConfig;
    
    List<IDisposable> _disposables = new();

    void Awake() {
        RegisterServices();
        Init();
        AddDisposables();
    }
    
    void RegisterServices() {
        ServiceLocator.Initialize();
        
        ServiceLocator.Current.Register(_playerConfig);
        ServiceLocator.Current.Register(_playerSpawnService);
        ServiceLocator.Current.Register(_inputService);
        ServiceLocator.Current.Register(_cameraSpawnService);
        ServiceLocator.Current.Register(_cursorService);
        ServiceLocator.Current.Register(_flyweightFactory);
    }

    async void Init() {
        await WaitForInstanceAsync();
        
        _playerSpawnService.Initialize(_playerPrefab);
        _inputService.Initialize();
        _cameraSpawnService.Initialize(_cameraPrefab);
        _cursorService.Initialize();
        
        Debug.Log("Service initialized");
    }

    void AddDisposables() {
        _disposables.Add(_inputService);
        _disposables.Add(_playerSpawnService);
        _disposables.Add(_cameraSpawnService);
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