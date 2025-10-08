using System.Collections.Generic;
using R3;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

namespace _PrismWars._Scripts.Components.Projectile {
    [RequireComponent(typeof(NetworkObject))]
    public class ProjectileFactory : NetworkBehaviour, IService {
        [SerializeField] Projectile _projectilePrefab;
        [SerializeField] bool _collectionCheck = true;
        [SerializeField] int _defaultCapacity = 10;
        [SerializeField] int _maxPoolSize = 100;

        readonly Dictionary<ProjectileType, IObjectPool<Projectile>> _pools = new();
        
        public readonly Subject<NetworkObjectReference> OnGetProjectile = new();
        public readonly Subject<NetworkObjectReference> OnReleaseProjectile = new();
        public readonly Subject<NetworkObjectReference> OnDestroyPoolObjectProjectile = new();

        public Projectile Spawn(Vector3 position, Vector3 direction) {
            var projectile = GetPoolFor()?.Get();
            projectile?.Initialize(position, direction);
            return projectile;
        }

        public void ReturnToPool(Projectile f) {
            if (IsServer)
                GetPoolFor()?.Release(f);
        }

        IObjectPool<Projectile> GetPoolFor()
        {
            IObjectPool<Projectile> pool;

            if (_pools.TryGetValue(_projectilePrefab.Type, out pool)) return pool;

            pool = new ObjectPool<Projectile>(
                Create,
                OnGet,
                OnRelease,
                OnDestroyPoolObject,
                _collectionCheck,
                _defaultCapacity,
                _maxPoolSize);
            _pools.Add(_projectilePrefab.Type, pool);
            return pool;
        }

        Projectile Create() {
            Projectile projectile = Instantiate(_projectilePrefab);
            projectile.gameObject.TryGetComponent(out NetworkObject networkObject);
            networkObject.Spawn(true);
            
            return projectile;
        }

        void OnGet(Projectile p) {
            if (IsServer) 
                OnGetRpc(p.NetworkObject);
        }

        void OnRelease(Projectile p) {
            if (IsServer) 
                OnOnReleaseRpc(p.NetworkObject);
        }

        void OnDestroyPoolObject(Projectile p) {
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