using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Components.Projectile {
    public class Projectile : NetworkBehaviour, IInitializable<Vector2, Vector2> {
        [SerializeField] float _speed;
        [SerializeField] float _damage;
        [SerializeField] float _despawnDelay = 5f;
        
        public ProjectileType Type;
        Vector2 _direction;

        public override void OnNetworkSpawn() {
            gameObject.SetActive(false);
        }

        void OnEnable() {
            StartCoroutine(DespawnAfterDelay(_despawnDelay));
        }

        public void Initialize(Vector2 startPos, Vector2 direction) {
            _direction = direction;
            transform.position = startPos;
            gameObject.layer = LayerMask.NameToLayer(Type.ToString());
            gameObject.SetActive(true);
        }
        
        void Update() {
            if (IsServer)
                transform.Translate(_direction * (_speed * Time.deltaTime));
        }

        void OnTriggerEnter2D(Collider2D other) {
            StopAllCoroutines();
            if (other.gameObject.layer != gameObject.layer) {
                other.GetComponent<IDamageable>()?.TakeDamage(_damage);
            }
            ReturnToPoolRpc();
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
        Fire,
        Ice
    }
}