using System;
using R3;
using Unity.Netcode;
using Object = UnityEngine.Object;

namespace _PrismWars._Scripts.Components.Projectile {
    public class ShardServerService : IService, IInitializable, IDisposable {
        NetworkObjectReference _networkObject;
        
        readonly CompositeDisposable _disposables = new();
        
        public void Initialize() {
            ShardFactory shardFactory = ServiceLocator.Singleton.Get<ShardFactory>();
            
            shardFactory.OnGetProjectile
                .Subscribe(OnGetShard)
                .AddTo(_disposables);
            shardFactory.OnReleaseProjectile
                .Subscribe(OnReleaseShard)
                .AddTo(_disposables);
            shardFactory.OnDestroyPoolObjectProjectile
                .Subscribe(OnDestroyPoolObjectShard)
                .AddTo(_disposables);
        }

        void OnGetShard(NetworkObjectReference nor) {
            Observable.EveryUpdate()
                .Select(_ => nor.TryGet(out NetworkObject networkObject) ? networkObject : null)
                .Where(networkObject => networkObject != null)
                .Take(1)
                .Subscribe(shard => shard.gameObject.SetActive(true));
        }
        void OnReleaseShard(NetworkObjectReference nor) {
            Observable.EveryUpdate()
                .Select(_ => nor.TryGet(out NetworkObject networkObject) ? networkObject : null)
                .Where(networkObject => networkObject != null)
                .Take(1)
                .Where(p => p.gameObject.activeSelf)
                .Subscribe(shard => shard.gameObject.SetActive(false));
        }
        void OnDestroyPoolObjectShard(NetworkObjectReference nor) {
            Observable.EveryUpdate()
                .Select(_ => nor.TryGet(out NetworkObject networkObject) ? networkObject : null)
                .Where(networkObject => networkObject != null)
                .Take(1)
                .Subscribe(shard => Object.Destroy(shard.gameObject));
        }

        public void Dispose() => _disposables.Dispose();
    }
}