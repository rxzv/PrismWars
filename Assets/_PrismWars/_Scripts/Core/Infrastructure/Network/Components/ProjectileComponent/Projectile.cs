using System.Collections;
using _PrismWars._Scripts.Core.Infrastructure.Interfaces;
using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Health;
using _PrismWars._Scripts.Core.Patterns.Factory.PoolFactory;
using _PrismWars._Scripts.Game.Player.Model;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Infrastructure.Network.Components.ProjectileComponent {
    public class Projectile : NetworkBehaviour, IPoolObject {
        [SerializeField] float _speed;
        [SerializeField] float _damage;
        [SerializeField] float _despawnDelay = 5f;
        
        NetworkVariable<ulong> _playerId = new();
        
        SpriteRenderer _spriteRenderer;
        
        NetworkVariable<PlayerElement> _type = new();
        
        Vector2 _direction;

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            gameObject.SetActive(false);
            Initialize();
        }

        void OnEnable() {
            StartCoroutine(DespawnAfterDelay(_despawnDelay));
        }

        public void SetType(PlayerElement element, ulong playerId) {
            _playerId.Value = playerId;
            _type.Value = element;
            Initialize();
        }

        void Initialize() {
            gameObject.layer = LayerMask.NameToLayer(_type.Value.ToString());
            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            _spriteRenderer.material = GetMaterialByPlayerType(_type.Value);
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
                other.GetComponent<IDamageable>()?.TakeDamage(_type.Value, _damage, _playerId.Value);
            }
            ReturnToPoolRpc(_type.Value);
        }

        IEnumerator DespawnAfterDelay(float delay) {
            yield return new WaitForSeconds(delay);
            ReturnToPoolRpc(_type.Value);
        }
        
        [Rpc(SendTo.Server)]
        void ReturnToPoolRpc(PlayerElement element) {
            ServiceLocator.Singleton.Get<ProjectileFactory>().ReturnToPool(this, element);
        }

        Material GetMaterialByPlayerType(PlayerElement playerElement) {
            return playerElement switch {
                PlayerElement.Fire => Resources.Load<Material>($"Materials/FireColorMaterial"),
                PlayerElement.Ice => Resources.Load<Material>($"Materials/IceColorMaterial"),
                _ => null
            };
        }
    }
}