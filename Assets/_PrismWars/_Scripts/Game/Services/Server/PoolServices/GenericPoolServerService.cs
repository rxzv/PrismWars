using System;
using _PrismWars._Scripts.Core.Infrastructure.Interfaces;
using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Services;
using R3;
using Unity.Netcode;
using Object = UnityEngine.Object;

namespace _PrismWars._Scripts.Game.Services.Server.PoolServices {
    public class GenericPoolServerService : IInitializable, IDisposable {
        readonly CompositeDisposable _disposables = new();
        readonly IPoolEventSource _factory;

        public GenericPoolServerService(IPoolEventSource factory) {
            _factory = factory;
        }

        public void Initialize() {            
            _factory.OnGetPoolObject
                .Subscribe(OnGetPoolObject)
                .AddTo(_disposables);
            
            _factory.OnReleasePoolObject
                .Subscribe(OnReleasePoolObject)
                .AddTo(_disposables);
            
            _factory.OnDestroyPoolObject
                .Subscribe(OnDestroyPoolObject)
                .AddTo(_disposables);
        }

        protected virtual void OnGetPoolObject(NetworkObjectReference nor) {
            Observable.EveryUpdate()
                .Select(_ => nor.TryGet(out NetworkObject networkObject) ? networkObject : null)
                .Where(networkObject => networkObject != null)
                .Take(1)
                .Subscribe(networkObject => networkObject.gameObject.SetActive(true));
        }

        protected virtual void OnReleasePoolObject(NetworkObjectReference nor) {
            Observable.EveryUpdate()
                .Select(_ => nor.TryGet(out NetworkObject networkObject) ? networkObject : null)
                .Where(networkObject => networkObject != null)
                .Take(1)
                .Where(p => p.gameObject.activeSelf)
                .Subscribe(networkObject => networkObject.gameObject.SetActive(false));
        }

        protected virtual void OnDestroyPoolObject(NetworkObjectReference nor) {
            Observable.EveryUpdate()
                .Select(_ => nor.TryGet(out NetworkObject networkObject) ? networkObject : null)
                .Where(networkObject => networkObject != null)
                .Take(1)
                .Subscribe(networkObject => Object.Destroy(networkObject.gameObject));
        }

        public void Dispose() => _disposables.Dispose();
    }
}