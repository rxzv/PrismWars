using System;
using System.Collections;
using _PrismWars._Scripts.Components.Projectile;
using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Infrastructure.Network.Components.Shard {
    public class Shard : NetworkBehaviour {
        [Header("Option")]
        [SerializeField] float _despawnDelay = 5f;
        [SerializeField] float _jumpForce = 10f;
        [SerializeField] float _bounceFactor = 0.8f;
        [SerializeField] LayerMask _groundLayer;
        [SerializeField] float _groundCheckDistance = 0.1f;
        [SerializeField] Vector2 _groundCheckSize = new Vector2(0.5f, 0.1f);
        [SerializeField] float _bounceDecay = 0.2f;
        
        bool _isGrounded;
        Vector2 _groundCheckPosition;
        private float _currentJumpForce;
        
        SpriteRenderer _spriteRenderer;
        
        Rigidbody2D _rb;
        
        NetworkVariable<PlayerElement> _type = new();
        public PlayerElement Element => _type.Value;
        
        Vector2 _direction;

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            _rb = GetComponent<NetworkRigidbody2D>().Rigidbody2D;
            if (_rb != null)
            {
                _rb.gravityScale = 1f;
                _rb.freezeRotation = true; 
            }
            
            gameObject.SetActive(false);
            Initialize();
        }

        void OnEnable() {
            ResetBounce();
            StartCoroutine(DespawnAfterDelay(_despawnDelay));
        }
        void ResetBounce() {
            _currentJumpForce = _jumpForce;
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
        void FixedUpdate()
        {
            if (IsServer) {
                CheckGrounded();
                if (_isGrounded) 
                    Jump();
            }
        }
        void CheckGrounded() {
            _groundCheckPosition = (Vector2)transform.position + Vector2.down * 
                (GetComponent<Collider2D>().bounds.extents.y + 0.01f);
        
            RaycastHit2D hit = Physics2D.BoxCast(
                _groundCheckPosition, 
                _groundCheckSize, 
                0f, 
                Vector2.down, 
                _groundCheckDistance, 
                _groundLayer
            );
        
            _isGrounded = hit.collider != null;
        }

        void Jump() {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f);
            _rb.AddForce(Vector2.up * _currentJumpForce, ForceMode2D.Impulse);
            CalculateNextJumpForce();
        }
        void CalculateNextJumpForce() {
            _currentJumpForce *= (1f - _bounceDecay);
        }

        void OnCollisionEnter2D(Collision2D other) {
            // if ground
            if (((1 << other.gameObject.layer) & _groundLayer) != 0) {
                Vector2 normal = other.contacts[0].normal;
            
                if (normal.y > 0.5f) {
                    Vector2 velocity = _rb.linearVelocity;
                    velocity.y = -velocity.y * _bounceFactor;
                    _rb.linearVelocity = velocity;
                }
            }
            // if player
            if (other.gameObject.layer != gameObject.layer) {
                var shard = other.gameObject.GetComponent<ShardComponent>();
                if (shard != null) {
                    StopAllCoroutines();
                    shard.AddShardServerRpc();
                    ReturnToPoolRpc(Element);
                }
            }
        }
        
        IEnumerator DespawnAfterDelay(float delay) {
            yield return new WaitForSeconds(delay);
            ReturnToPoolRpc(Element);
        }
        
        [Rpc(SendTo.Server)]
        void ReturnToPoolRpc(PlayerElement element) {
            ServiceLocator.Singleton.Get<ShardFactory>().ReturnToPool(this, element);
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