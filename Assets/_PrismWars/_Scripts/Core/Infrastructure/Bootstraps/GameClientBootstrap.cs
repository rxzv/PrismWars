using System;
using System.Collections.Generic;
using _PrismWars._Scripts.Core.Infrastructure.Bootstraps;
using _PrismWars._Scripts.Systems;
using _PrismWars._Scripts.UI;
using UnityEngine;

public class GameClientBootstrap : MonoBehaviour {
    [Header("Client Services")] 
    [SerializeField] CameraSpawnService _cameraSpawnService;
    [SerializeField] InputService _inputService;
    [SerializeField] CursorService _cursorService;
    
    
    List<IDisposable> _disposables = new();
    void Start() {
        RegisterClientServices();
        InitializeClientServices();
        AddClientDisposables();
    }
    
    void RegisterClientServices() {
        ServiceLocator.Singleton.Register(_inputService);
        ServiceLocator.Singleton.Register(_cameraSpawnService);
        ServiceLocator.Singleton.Register(_cursorService);
        
        Debug.Log("Client services registered");
    }
    void InitializeClientServices() {
        var cameraPrefab = NetworkGameInstaller.Singleton.CameraPrefab;
        _inputService.Initialize();
        _cameraSpawnService.Initialize(cameraPrefab);
        _cursorService.Initialize();
    }
    void AddClientDisposables() {
        _disposables.Add(_inputService);
        _disposables.Add(_cameraSpawnService);
    }

    void OnDestroy() {
        foreach (var disposable in _disposables) {
            disposable.Dispose();
        }
    }
}