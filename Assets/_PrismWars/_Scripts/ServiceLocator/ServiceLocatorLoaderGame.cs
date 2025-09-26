using System.Collections.Generic;
using System.Threading.Tasks;
using _PrismWars._Scripts;
using Unity.Netcode;
using UnityEngine;

public class ServiceLocatorLoaderGame : MonoBehaviour {

    [SerializeField] CameraSpawnService _cameraSpawnService;
    [SerializeField] InputService _inputService;
    [SerializeField] PlayerSpawnService _playerSpawnService;    
    
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
        _playerSpawnService.Init();
        _inputService.Init();
        _cameraSpawnService.Init();
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