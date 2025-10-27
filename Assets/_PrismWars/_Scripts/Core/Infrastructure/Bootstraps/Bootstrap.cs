using System;
using System.Collections.Generic;
using _PrismWars._Scripts.Components.Projectile;
using _PrismWars._Scripts.Game.Services;
using _PrismWars._Scripts.Systems;
using _PrismWars._Scripts.UI;
using _PrismWars._Scripts.Utils;
using UnityEngine;

public class Bootstrap : MonoBehaviour {
    [Header("Mono Services")] 
    [SerializeField] InputService _inputService;
    [SerializeField] CursorService _cursorService;
    
    [Header("Network Services")] 
    [SerializeField] NetworkTimer _networkTimer;
    [SerializeField] NetworkCharacterSelectionManager _networkCharacterSelectionManager;
    [SerializeField] ProjectileFactory _projectileFactory;
    [SerializeField] PlayerRespawnService _playerRespawnService;
    
    List<IDisposable> _disposables = new();
    
    void Start() {
        RegisterServices();
        InitializeServices();
    }
    
    void RegisterServices() {
        ServiceLocator.Singleton.Register(_inputService);
        ServiceLocator.Singleton.Register(_cursorService);
        ServiceLocator.Singleton.Register(_networkCharacterSelectionManager);
        ServiceLocator.Singleton.Register(_networkTimer);
        ServiceLocator.Singleton.Register(_projectileFactory);
        ServiceLocator.Singleton.Register(_playerRespawnService);
        
        DontDestroyOnLoad(_inputService);
        DontDestroyOnLoad(_cursorService);
        DontDestroyOnLoad(_projectileFactory);
        DontDestroyOnLoad(_networkCharacterSelectionManager);
        DontDestroyOnLoad(_networkTimer);
        DontDestroyOnLoad(_playerRespawnService);
        
        Debug.Log("BootstrapScene Services registered");
    }
    void InitializeServices() {
        var projectilePrefab = Resources.Load<Projectile>("Prefabs/Projectile");
        
        _inputService.Initialize();
        _cursorService.Initialize();
        _projectileFactory.Initialize(projectilePrefab);
        
        SceneLoader.LoadNetwork("Game");
        Debug.Log("BootstrapScene Services initialize");
    }
}