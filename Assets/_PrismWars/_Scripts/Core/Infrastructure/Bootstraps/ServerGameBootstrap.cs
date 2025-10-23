using System;
using System.Collections.Generic;
using _PrismWars._Scripts.Components.Projectile;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Infrastructure.Bootstraps {
    [RequireComponent(typeof(NetworkObject))]
    public class ServerGameBootstrap : NetworkBehaviour {
        ProjectileFactory _projectileFactory;
        ProjectileServerService _projectileServerService;
        
        List<IDisposable> _disposables = new();
        
        public override void OnNetworkSpawn() {
            if (IsServer) {
                RegisterServices();
                InitializeServices();
                AddDisposables();
            }
        }
        void RegisterServices() {
            var servicePrefab = Resources.Load<GameObject>("NetworkServicePrefabs/ProjectileFactory");
            var serviceGo2 = Instantiate(servicePrefab);
            serviceGo2.GetComponent<NetworkObject>().Spawn(false);
            _projectileFactory = serviceGo2.GetComponent<ProjectileFactory>();
            ServerServiceLocator.Singleton.Register(_projectileFactory);
            
            _projectileServerService = new ProjectileServerService();
            ServerServiceLocator.Singleton.Register(_projectileServerService);
        }
        void InitializeServices() {
            var projectilePrefab = Resources.Load<Projectile>("Prefabs/Projectile");
            
            _projectileFactory.Initialize(projectilePrefab);
            _projectileServerService.Initialize();
        }

        void AddDisposables() {
            _disposables.Add(_projectileServerService);
        }
        
        void OnDestroy() {
            foreach (var disposable in _disposables) {
                disposable.Dispose();
            }
        }
    }
}