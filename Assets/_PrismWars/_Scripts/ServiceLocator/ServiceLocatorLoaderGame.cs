using System.Collections.Generic;
using System.Threading.Tasks;
using _PrismWars._Scripts;
using _PrismWars._Scripts.Player;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class ServiceLocatorLoaderGame : MonoBehaviour {

    [Header("Services")]
    [SerializeField] CameraSpawnService _cameraSpawnService;
    [SerializeField] InputService _inputService;
    [SerializeField] PlayerSpawnService _playerSpawnService;    
    
    [Header("Prefabs")]
    [SerializeField] Transform _playerPrefab;
    [SerializeField] CinemachineCamera _cameraPrefab;
    
    [Header("Configs")]
    [SerializeField] PlayerConfig _playerConfig;
    
    List<IService> _disposables = new();

    void Awake()
    {
        RegisterServices();
        Init();
        AddDisposables();
    }
    
    void RegisterServices()
    {
        ServiceLocator.Initialize();
        
        ServiceLocator.Current.Register(_playerSpawnService);
        ServiceLocator.Current.Register(_inputService);
        ServiceLocator.Current.Register(_cameraSpawnService);
    }

    async void Init() {
        await WaitForInstanceAsync();
        _playerSpawnService.Initialize(_playerPrefab, _playerConfig);
        _inputService.Initialize();
        _cameraSpawnService.Initialize(_cameraPrefab);
        Debug.Log("Service initialized");
    }

    void AddDisposables()
    {
        _disposables.Add(_inputService);
        _disposables.Add(_playerSpawnService);
        _disposables.Add(_cameraSpawnService);
    }
    private async Task WaitForInstanceAsync()
    {
        while (NetworkManager.Singleton == null)
        {
            await Task.Yield();
        }
    }

    void OnDestroy()
    {
        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }
    }
}