using System;
using System.Collections.Generic;
using _PrismWars._Scripts.Components.Projectile;
using _PrismWars._Scripts.Game.GameManager;
using _PrismWars._Scripts.UI;
using _PrismWars._Scripts.Utils;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Infrastructure.Bootstraps {
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkGameInstaller : NetworkBehaviour {
        public static NetworkGameInstaller Singleton { get; private set; }
        
        ServerGameManager _serverGameManager;
        ProjectileFactory _projectileFactory;
        NetworkTimer _networkTimer;
        CharacterSelectionManager _characterSelectionManager;
        ProjectileService _projectileService;
        
        Projectile _projectilePrefab;
        public CinemachineCamera CameraPrefab { get; private set; }
        
        List<IDisposable> _disposables = new();
        
        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            if (IsServer) {
                if (Singleton == null) 
                    Singleton = this;
                else 
                    Debug.LogWarning("[NetworkGameInstaller] Attempting to spawn a server game instance.");

                InstallServices();
                InitializeServices();
                AddDisposables();
            }
        }

        void InstallServices() {
            _projectilePrefab = Resources.Load<Projectile>("Prefabs/Projectile");
            CameraPrefab = Resources.Load<CinemachineCamera>("Prefabs/CinemachineCamera");

            var servicePrefab = Resources.Load<GameObject>("NetworkServicePrefabs/ServerGameManager");
            var serviceGo = Instantiate(servicePrefab);
            serviceGo.GetComponent<NetworkObject>().Spawn(false);
            _serverGameManager = serviceGo.GetComponent<ServerGameManager>();
            ServiceLocator.Singleton.Register(_serverGameManager);
            
            servicePrefab = Resources.Load<GameObject>("NetworkServicePrefabs/ProjectileFactory");
            var serviceGo2 = Instantiate(servicePrefab);
            serviceGo2.GetComponent<NetworkObject>().Spawn(false);
            _projectileFactory = serviceGo2.GetComponent<ProjectileFactory>();
            ServiceLocator.Singleton.Register(_projectileFactory);
            
            servicePrefab = Resources.Load<GameObject>("NetworkServicePrefabs/NetworkTimer");
            var serviceGo3 = Instantiate(servicePrefab);
            serviceGo3.GetComponent<NetworkObject>().Spawn(false);
            _networkTimer = serviceGo3.GetComponent<NetworkTimer>();
            ServiceLocator.Singleton.Register(_networkTimer);
            
            servicePrefab = Resources.Load<GameObject>("NetworkServicePrefabs/CharacterSelectionManager");
            var serviceGo4 = Instantiate(servicePrefab);
            serviceGo4.GetComponent<NetworkObject>().Spawn(false);
            _characterSelectionManager = serviceGo3.GetComponent<CharacterSelectionManager>();
            ServiceLocator.Singleton.Register(_characterSelectionManager);
            
            _projectileService = new ProjectileService();
            ServiceLocator.Singleton.Register(_projectileService);
        }

        void InitializeServices() {
            _projectileFactory.Initialize(_projectilePrefab);
            _serverGameManager.Initialize();
        }

         void AddDisposables() {
            _disposables.Add(_projectileService);
        }
        void OnDestroy() {
            foreach (var disposable in _disposables) {
                disposable.Dispose();
            }
        }
    }
}