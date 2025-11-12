using System.Collections;
using _PrismWars._Scripts.Core.Infrastructure.Interfaces;
using _PrismWars._Scripts.Core.Patterns.Factory.PoolFactory;
using _PrismWars._Scripts.Game.Player.Controllers;
using _PrismWars._Scripts.Game.Player.Model;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Infrastructure.Network.Components.Shard {
    public class Shard : NetworkBehaviour, IPoolObject {
        [SerializeField] float _despawnDelay = 5f;
        
        SpriteRenderer _spriteRenderer;
        
        NetworkVariable<PlayerElement> _type = new();

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            gameObject.SetActive(false);
            Initialize();
        }

        void OnEnable() {
            StartCoroutine(DespawnAfterDelay(_despawnDelay));
        }

        public void SetType(PlayerElement element, ulong playerId) {
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
            gameObject.SetActive(true);
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (other.gameObject.layer != gameObject.layer) {
                StopAllCoroutines();
                ReturnToPoolRpc(_type.Value);
                var shard = other.GetComponent<ShardController>();
                if (shard != null) {
                    shard.AddShardServerRpc();
                }
            }
        }
        IEnumerator DespawnAfterDelay(float delay) {
            yield return new WaitForSeconds(delay);
            ReturnToPoolRpc(_type.Value);
        }
        
        [Rpc(SendTo.Server)]
        void ReturnToPoolRpc(PlayerElement element) {
            ServiceLocator.Singleton.Get<ShardFactory>().ReturnToPool(this, element);
        }

        Material GetMaterialByPlayerType(PlayerElement playerElement) {
            return playerElement switch {
                PlayerElement.Fire => Resources.Load<Material>("Materials/FireColorMaterial"),
                PlayerElement.Ice => Resources.Load<Material>("Materials/IceColorMaterial"),
                _ => null
            };
        }
    }
}