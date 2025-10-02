
using System;
using R3;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _PrismWars._Scripts.Components.Projectile {
    public class ProjectileService : IService, IInitializable, IDisposable {
        NetworkObjectReference _networkObject;
        
        readonly CompositeDisposable _disposables = new();
        
        public void Initialize() {
            ProjectileFactory projectileFactory = ServiceLocator.Current.Get<ProjectileFactory>();
            
            projectileFactory.OnGetProjectile
                .Subscribe(OnGetProjectile)
                .AddTo(_disposables);
            projectileFactory.OnReleaseProjectile
                .Subscribe(OnReleaseProjectile)
                .AddTo(_disposables);
            projectileFactory.OnDestroyPoolObjectProjectile
                .Subscribe(OnDestroyPoolObjectProjectile)
                .AddTo(_disposables);
        }

        void OnGetProjectile(NetworkObjectReference nor) {
            Observable.EveryUpdate()
                .Select(_ => nor.TryGet(out NetworkObject playerObject) ? playerObject : null)
                .Where(playerObject => playerObject != null)
                .Take(1)
                .Subscribe(player => player.gameObject.SetActive(true));
        }
        void OnReleaseProjectile(NetworkObjectReference nor) {
            Observable.EveryUpdate()
                .Select(_ => nor.TryGet(out NetworkObject playerObject) ? playerObject : null)
                .Where(playerObject => playerObject != null)
                .Take(1)
                .Subscribe(player => player.gameObject.SetActive(false));
        }
        void OnDestroyPoolObjectProjectile(NetworkObjectReference nor) {
            Observable.EveryUpdate()
                .Select(_ => nor.TryGet(out NetworkObject playerObject) ? playerObject : null)
                .Where(playerObject => playerObject != null)
                .Take(1)
                .Subscribe(player => Object.Destroy(player.gameObject));
        }

        public void Dispose() => _disposables.Dispose();
    }
}