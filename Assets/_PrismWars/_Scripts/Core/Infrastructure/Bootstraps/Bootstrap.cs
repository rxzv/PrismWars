using System;
using System.Collections.Generic;
using _PrismWars._Scripts.Components.Projectile;
using _PrismWars._Scripts.Core.Infrastructure.Network.Components.Shard;
using _PrismWars._Scripts.Game.Services;
using _PrismWars._Scripts.Systems;
using _PrismWars._Scripts.UI;
using _PrismWars._Scripts.Utils;
using UnityEngine;

public class Bootstrap : MonoBehaviour {
    [Header("Mono Services")] 
    [SerializeField] InputService _inputService;
    [SerializeField] CursorService _cursorService;
    [SerializeField] RespawnTimer _respawnTimer;
    
    [Header("Network Services")] 
    [SerializeField] NetworkGameTimer _networkGameTimer;
    [SerializeField] NetworkCharacterSelectionManager _networkCharacterSelectionManager;
    [SerializeField] ProjectileFactory _projectileFactory;
    [SerializeField] NetworkScoreManager  _networkScoreManager;
    [SerializeField] ShardFactory _shardFactory;
    
    List<IDisposable> _disposables = new();
    
    void Start() {
        RegisterServices();
        InitializeServices();
    }
    
    void RegisterServices() {
        ServiceLocator.Singleton.Register(_inputService);
        ServiceLocator.Singleton.Register(_cursorService);
        ServiceLocator.Singleton.Register(_networkCharacterSelectionManager);
        ServiceLocator.Singleton.Register(_networkGameTimer);
        ServiceLocator.Singleton.Register(_projectileFactory);
        ServiceLocator.Singleton.Register(_shardFactory);
        ServiceLocator.Singleton.Register(_respawnTimer);
        ServiceLocator.Singleton.Register(_networkScoreManager);
        
        DontDestroyOnLoad(_inputService);
        DontDestroyOnLoad(_cursorService);
        DontDestroyOnLoad(_projectileFactory);
        DontDestroyOnLoad(_networkCharacterSelectionManager);
        DontDestroyOnLoad(_networkGameTimer);
        DontDestroyOnLoad(_respawnTimer);
        DontDestroyOnLoad(_networkScoreManager);
        DontDestroyOnLoad(_shardFactory);
        
        Debug.Log("BootstrapScene Services registered");
    }
    void InitializeServices() {
        var projectilePrefab = Resources.Load<Projectile>("Prefabs/Projectile");
        var shardPrefab = Resources.Load<Shard>("Prefabs/Shard");
        
        _inputService.Initialize();
        _cursorService.Initialize();
        _projectileFactory.Initialize(projectilePrefab);
        _shardFactory.Initialize(shardPrefab);
        
        SceneLoader.LoadNetwork("Game");
        Debug.Log("BootstrapScene Services initialize");
    }
}