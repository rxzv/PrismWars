using System.Collections.Generic;
using _PrismWars._Scripts.UI.Model;
using R3;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

namespace _PrismWars._Scripts.Components.Projectile {
    [RequireComponent(typeof(NetworkObject))]
    public class ProjectileFactory : NetworkBehaviour, IServerService, IInitializable<Projectile> {
        [SerializeField] bool _collectionCheck = true;
        [SerializeField] int _defaultCapacity = 10;
        [SerializeField] int _maxPoolSize = 100;
        
        NetworkVariable<PlayerElement> _currentType = new();
        Projectile _projectilePrefab;

        readonly Dictionary<PlayerElement, IObjectPool<Projectile>> _pools = new();
        
        public readonly Subject<NetworkObjectReference> OnGetProjectile = new();
        public readonly Subject<NetworkObjectReference> OnReleaseProjectile = new();
        public readonly Subject<NetworkObjectReference> OnDestroyPoolObjectProjectile = new();
        private IInitializable<Projectile> _initializableImplementation;

        public void Initialize(Projectile projectilePrefab) {
            _projectilePrefab = projectilePrefab;
        }
        
        public Projectile Spawn(Vector3 position, Vector3 direction, PlayerElement element) {
            var projectile = GetPoolFor(element)?.Get();
            projectile?.SetPosition(position, direction);
            return projectile;
        }

        public void ReturnToPool(Projectile f, PlayerElement element) {
            if (!IsServer) return;
            if(f.gameObject.activeSelf)
                GetPoolFor(element)?.Release(f);
        }

        IObjectPool<Projectile> GetPoolFor(PlayerElement element) {
            IObjectPool<Projectile> pool;
            _currentType.Value = element;
            
            if (_pools.TryGetValue(element, out pool)) return pool;

            pool = new ObjectPool<Projectile>(
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

        Projectile Create() {
            Projectile projectile = Instantiate(_projectilePrefab);
            projectile.SetType(_currentType.Value);
            
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