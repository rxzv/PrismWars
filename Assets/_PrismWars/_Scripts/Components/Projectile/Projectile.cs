using System.Collections;
using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Components.Projectile {
    public class Projectile : NetworkBehaviour, IInitializable<PlayerType> {
        [SerializeField] float _speed;
        [SerializeField] float _damage;
        [SerializeField] float _despawnDelay = 5f;
        
        NetworkVariable<PlayerType> _type = new();
        public PlayerType Type => _type.Value;
        
        Vector2 _direction;

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            gameObject.SetActive(false);
            
            gameObject.layer = LayerMask.NameToLayer(Type.ToString());
        }

        void OnEnable() {
            StartCoroutine(DespawnAfterDelay(_despawnDelay));
        }

        public void Initialize(PlayerType type) {
            _type.Value = type;
            gameObject.layer = LayerMask.NameToLayer(Type.ToString());
        }


        public void SetPosition(Vector2 startPos, Vector2 direction) {
            transform.position = startPos;
            _direction = direction;
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
            ReturnToPoolRpc(Type);
        }

        IEnumerator DespawnAfterDelay(float delay) {
            yield return new WaitForSeconds(delay);
            ReturnToPoolRpc(Type);
        }
        
        [Rpc(SendTo.Server)]
        void ReturnToPoolRpc(PlayerType type) {
            ServiceLocator.Current.Get<ProjectileFactory>().ReturnToPool(this, type);
        }
    }
}