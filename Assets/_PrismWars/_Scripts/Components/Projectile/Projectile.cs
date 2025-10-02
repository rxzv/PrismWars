using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Components.Projectile {
    public class Projectile : NetworkBehaviour, IInitializable<Vector2, Vector2> {
        [SerializeField] float _speed;
        [SerializeField] float _damage;
        [SerializeField] float _despawnDelay = 5f;
        [SerializeField] ProjectileType _type;

        Vector2 _direction;
        
        public ProjectileType Type => _type;

        void Start() {
            gameObject.SetActive(false);
        }

        void OnEnable() {
            StartCoroutine(DespawnAfterDelay(_despawnDelay));
        }

        public void Initialize(Vector2 startPos, Vector2 direction) {
            _direction = direction;
            transform.position = startPos;
            gameObject.SetActive(true);
        }
        
        void Update() {
            if (IsServer)
                transform.Translate(_direction * (_speed * Time.deltaTime));
        }
        
        IEnumerator DespawnAfterDelay(float delay) {
            yield return new WaitForSeconds(delay);
            ReturnToPoolRpc();
        }
        
        [Rpc(SendTo.Server)]
        void ReturnToPoolRpc() {
            ServiceLocator.Current.Get<ProjectileFactory>().ReturnToPool(this);
        }
    }

    public enum ProjectileType {
        None
    }
}