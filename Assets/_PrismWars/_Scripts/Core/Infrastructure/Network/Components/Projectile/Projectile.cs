using System.Collections;
using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Components.Projectile {
    public class Projectile : NetworkBehaviour {
        [SerializeField] float _speed;
        [SerializeField] float _damage;
        [SerializeField] float _despawnDelay = 5f;
        
        SpriteRenderer _spriteRenderer;
        
        NetworkVariable<PlayerType> _type = new();
        public PlayerType Type => _type.Value;
        
        Vector2 _direction;

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            gameObject.SetActive(false);
            Initialize();
        }

        void OnEnable() {
            StartCoroutine(DespawnAfterDelay(_despawnDelay));
        }

        public void SetType(PlayerType type) {
            _type.Value = type;
            Initialize();
        }

        void Initialize() {
            gameObject.layer = LayerMask.NameToLayer(Type.ToString());
            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            _spriteRenderer.material = GetMaterialByPlayerType(Type);
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
            ServiceLocator.Singleton.Get<ProjectileFactory>().ReturnToPool(this, type);
        }

        Material GetMaterialByPlayerType(PlayerType playerType) {
            return playerType switch {
                PlayerType.Fire => Resources.Load<Material>($"Materials/FireColorMaterial"),
                PlayerType.Ice => Resources.Load<Material>($"Materials/IceColorMaterial"),
                _ => default
            };
        }
    }
}