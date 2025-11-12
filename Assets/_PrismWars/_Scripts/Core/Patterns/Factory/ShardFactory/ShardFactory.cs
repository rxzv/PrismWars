using System.Collections.Generic;
using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Services;
using _PrismWars._Scripts.Core.Infrastructure.Network.Components.Shard;
using _PrismWars._Scripts.UI.Model;
using R3;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

namespace _PrismWars._Scripts.Core.Patterns.Factory.ShardFactory {
    [RequireComponent(typeof(NetworkObject))]
    public class ShardFactory : NetworkBehaviour, IService, IInitializable<Shard> {
        [SerializeField] bool _collectionCheck = true;
        [SerializeField] int _defaultCapacity = 10;
        [SerializeField] int _maxPoolSize = 100;
        
        NetworkVariable<PlayerElement> _currentType = new();
        Shard _shardPrefab;

        readonly Dictionary<PlayerElement, IObjectPool<Shard>> _pools = new();
        
        public readonly Subject<NetworkObjectReference> OnGetProjectile = new();
        public readonly Subject<NetworkObjectReference> OnReleaseProjectile = new();
        public readonly Subject<NetworkObjectReference> OnDestroyPoolObjectProjectile = new();

        public void Initialize(Shard shardPrefab) {
            _shardPrefab = shardPrefab;
        }
        
        public Shard Spawn(Vector3 position, Vector3 direction, PlayerElement element) {
            var shard = GetPoolFor(element)?.Get();
            shard?.SetPosition(position, direction);
            return shard;
        }

        public void ReturnToPool(Shard s, PlayerElement element) {
            if (!IsServer) return;
            if(s.gameObject.activeSelf)
                GetPoolFor(element)?.Release(s);
        }

        IObjectPool<Shard> GetPoolFor(PlayerElement element) {
            IObjectPool<Shard> pool;
            _currentType.Value = element;
            
            if (_pools.TryGetValue(element, out pool)) return pool;

            pool = new ObjectPool<Shard>(
                Create,
                OnGet,
                OnRelease,
                OnDestroyPoolObject,
                _collectionCheck,
                _defaultCapacity,
                _maxPoolSize);
            _pools.Add(element, pool);
            return pool;
        }

        Shard Create() {
            Shard projectile = Instantiate(_shardPrefab);
            projectile.SetType(_currentType.Value);
            
            projectile.gameObject.TryGetComponent(out NetworkObject networkObject);
            networkObject.Spawn(true);
            
            return projectile;
        }

        void OnGet(Shard p) {
            if (IsServer) 
                OnGetRpc(p.NetworkObject);
        }

        void OnRelease(Shard p) {
            if (IsServer) 
                OnOnReleaseRpc(p.NetworkObject);
        }

        void OnDestroyPoolObject(Shard p) {
            if (IsServer)
                OnDestroyPoolObjectRpc(p.NetworkObject);
        }
        
        [Rpc(SendTo.ClientsAndHost)]
        void OnGetRpc(NetworkObjectReference p) => OnGetProjectile?.OnNext(p);
        
        [Rpc(SendTo.ClientsAndHost)]
        void OnOnReleaseRpc(NetworkObjectReference p) => OnReleaseProjectile?.OnNext(p);
        
        [Rpc(SendTo.ClientsAndHost)]
        void OnDestroyPoolObjectRpc(NetworkObjectReference p) => OnDestroyPoolObjectProjectile?.OnNext(p);

    }
}