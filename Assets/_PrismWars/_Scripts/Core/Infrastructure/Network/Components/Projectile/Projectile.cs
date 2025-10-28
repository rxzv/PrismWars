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
        
        NetworkVariable<PlayerElement> _type = new();
        public PlayerElement Element => _type.Value;
        
        Vector2 _direction;

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            gameObject.SetActive(false);
            Initialize();
        }

        void OnEnable() {
            StartCoroutine(DespawnAfterDelay(_despawnDelay));
        }

        public void SetType(PlayerElement element) {
            _type.Value = element;
            Initialize();
        }

        void Initialize() {
            gameObject.layer = LayerMask.NameToLayer(Element.ToString());
            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            _spriteRenderer.material = GetMaterialByPlayerType(Element);
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
                other.GetComponent<IDamageable>()?.TakeDamage(_type.Value, _damage);
            }
            ReturnToPoolRpc(Element);
        }

        IEnumerator DespawnAfterDelay(float delay) {
            yield return new WaitForSeconds(delay);
            ReturnToPoolRpc(Element);
        }
        
        [Rpc(SendTo.Server)]
        void ReturnToPoolRpc(PlayerElement element) {
            ServiceLocator.Singleton.Get<ProjectileFactory>().ReturnToPool(this, element);
        }

        Material GetMaterialByPlayerType(PlayerElement playerElement) {
            return playerElement switch {
                PlayerElement.Fire => Resources.Load<Material>($"Materials/FireColorMaterial"),
                PlayerElement.Ice => Resources.Load<Material>($"Materials/IceColorMaterial"),
                _ => default
            };
        }
    }
}