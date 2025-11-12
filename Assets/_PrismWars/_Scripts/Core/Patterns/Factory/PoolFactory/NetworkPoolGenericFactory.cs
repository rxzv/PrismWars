using System.Collections.Generic;
using _PrismWars._Scripts.Core.Infrastructure.Interfaces;
using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Services;
using _PrismWars._Scripts.Game.Player.Model;
using R3;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

namespace _PrismWars._Scripts.Core.Patterns.Factory.PoolFactory {
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkPoolGenericFactory<T> : NetworkBehaviour, IInitializable<T>, IPoolEventSource
        where T : NetworkBehaviour, IPoolObject {
        [SerializeField] protected bool _collectionCheck = true;
        [SerializeField] protected int _defaultCapacity = 10;
        [SerializeField] protected int _maxPoolSize = 100;
        
        NetworkVariable<PlayerElement> _currentType = new();
        NetworkVariable<ulong> _currentPlayerId = new();
        
        T _prefab;

        readonly Dictionary<PlayerElement, IObjectPool<T>> _pools = new();
        
        public readonly Subject<NetworkObjectReference> OnGetPoolObject = new();
        public readonly Subject<NetworkObjectReference> OnReleasePoolObject = new();
        public readonly Subject<NetworkObjectReference> OnDestroyPoolObject = new();
        Subject<NetworkObjectReference> IPoolEventSource.OnGetPoolObject => OnGetPoolObject;
        Subject<NetworkObjectReference> IPoolEventSource.OnReleasePoolObject => OnReleasePoolObject;
        Subject<NetworkObjectReference> IPoolEventSource.OnDestroyPoolObject => OnDestroyPoolObject;
        public void Initialize(T prefab) {
            _prefab = prefab;
        }
        public T Spawn(Vector3 position, Vector3 direction, PlayerElement element, ulong playerId) {
            _currentPlayerId.Value = playerId;
            var poolObject = GetPoolFor(element)?.Get();
            poolObject?.SetPosition(position, direction);
            return poolObject;
        }
        public void ReturnToPool(T s, PlayerElement element) {
            if (!IsServer) return;
            if(s.gameObject.activeSelf)
                GetPoolFor(element)?.Release(s);
        }
        IObjectPool<T> GetPoolFor(PlayerElement element) {
            IObjectPool<T> pool;
            _currentType.Value = element;
            
            if (_pools.TryGetValue(element, out pool)) return pool;

            pool = new ObjectPool<T>(
                Create,
                Get,
                Release,
                DestroyPoolObject,
                _collectionCheck,
                _defaultCapacity,
                _maxPoolSize);
            _pools.Add(element, pool);
            return pool;
        }
        T Create() {
            T instantiate = Instantiate(_prefab);
            instantiate.SetType(_currentType.Value, _currentPlayerId.Value);
            
            instantiate.gameObject.TryGetComponent(out NetworkObject networkObject);
            networkObject.Spawn(true);
            
            return instantiate;
        }
        void Get(T p) {
            if (IsServer) 
                GetRpc(p.NetworkObject);
        }

        void Release(T p) {
            if (IsServer) 
                ReleaseRpc(p.NetworkObject);
        }

        void DestroyPoolObject(T p) {
            if (IsServer)
                DestroyPoolObjectRpc(p.NetworkObject);
        }
         
        [Rpc(SendTo.ClientsAndHost)]
        void GetRpc(NetworkObjectReference p) => OnGetPoolObject?.OnNext(p);
        
        [Rpc(SendTo.ClientsAndHost)]
        void ReleaseRpc(NetworkObjectReference p) => OnReleasePoolObject?.OnNext(p);
        
        [Rpc(SendTo.ClientsAndHost)]
        void DestroyPoolObjectRpc(NetworkObjectReference p) => OnDestroyPoolObject?.OnNext(p);

    }
}