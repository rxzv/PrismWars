using System;
using System.Collections.Generic;
using _PrismWars._Scripts;
using _PrismWars._Scripts.Core.Patterns.Factory;
using _PrismWars._Scripts.Game.GameManager;
using _PrismWars._Scripts.Systems;
using _PrismWars._Scripts.UI;
using _PrismWars._Scripts.Utils;
using Unity.Cinemachine;
using UnityEngine;

public class ClientGameBootstrap : MonoBehaviour {
    [Header("Client MonoBehaviour Services")] 
    [SerializeField] ClientCameraSpawnService _clientCameraSpawnService;
    [SerializeField] ClientInputService _clientInputService;
    [SerializeField] ClientCursorService _clientCursorService;
    
    [Header("Client Network Services")] 
    [SerializeField] NetworkPlayerSpawnService _networkPlayerSpawnService;
    [SerializeField] NetworkCharacterSelectionManager _networkCharacterSelectionManager;
    [SerializeField] NetworkTimer _networkTimer;
    [SerializeField] NetworkUIManager _networkUIManager;
    [SerializeField] NetworkGameManager _networkGameManager;
    
    [Header("Scene Components")]
    [SerializeField] List<Transform> _fireSpawnPoints;
    [SerializeField] List<Transform> _iceSpawnPoints;
    
    List<IDisposable> _disposables = new();
    
    void Start() {
        RegisterServices();
        InitializeServices();
        AddDisposables();
    }
    
    void RegisterServices() {
        ClientServiceLocator.Singleton.Register(_networkGameManager);
        ClientServiceLocator.Singleton.Register(_networkPlayerSpawnService);
        ClientServiceLocator.Singleton.Register(_clientInputService);
        ClientServiceLocator.Singleton.Register(_clientCameraSpawnService);
        ClientServiceLocator.Singleton.Register(_clientCursorService);
        ClientServiceLocator.Singleton.Register(_networkUIManager);
        ClientServiceLocator.Singleton.Register(_networkCharacterSelectionManager);
        ClientServiceLocator.Singleton.Register(_networkTimer);
        
        Debug.Log("Client Game Services registered");
    }
    void InitializeServices() {
        var cameraPrefab = Resources.Load<CinemachineCamera>("Prefabs/CinemachineCamera");
        
        var playerPrefab = Resources.Load<Transform>("Prefabs/Player");
        var playerFireFactory = new PlayerFireFactory(playerPrefab, _fireSpawnPoints);
        var playerIceFactory = new PlayerIceFactory(playerPrefab, _iceSpawnPoints);
        
        _clientInputService.Initialize();
        _clientCameraSpawnService.Initialize(cameraPrefab);
        _clientCursorService.Initialize();
        _networkPlayerSpawnService.Initialize(playerFireFactory, playerIceFactory);
        _networkUIManager.Initialize();
        _networkGameManager.Initialize();
        
        Debug.Log("Client Game Services initialize");
    }
    void AddDisposables() {
        _disposables.Add(_clientInputService);
        _disposables.Add(_clientCameraSpawnService);
    }

    void OnDestroy() {
        foreach (var disposable in _disposables) {
            disposable.Dispose();
        }
    }
}