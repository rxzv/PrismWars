using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Services;
using _PrismWars._Scripts.Core.Infrastructure.Network.Components.Cat;
using _PrismWars._Scripts.Game.Player.Model;
using _PrismWars._Scripts.Player;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Game.Player.Controllers {
    public class CatCaptureController : NetworkBehaviour, IInitializable {
        const float THROW_FORCE = 100f;
        const string CAT_TAG = "Cat";
        const float CAT_IS_TALLER_THAN_PLAYER = 1.3f;

        bool _catPickUpArea;
        bool _catPickedUp;

        GameObject _catObj;
        Rigidbody2D _catObjRb;
        CatController _catController;
        SpriteRenderer _spriteRenderer;
        
        HealthController _healthController;
        PlayerElement _playerElement;
        
        public bool CatPickedUp => _catPickedUp;

        public void Initialize() {
            if (!IsOwner) return;
            _healthController = GetComponent<HealthController>();
            _playerElement = GetComponent<PlayerController>().PlayerElement.Value;
            _healthController!.OnDeath += PlayerIsDeath;
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void FlipXCat(float oldDir, float newDir) {
            if(!_catPickedUp || !IsOwner || _catObj == null || _catController == null) return;
            switch (newDir) {
                case > 0:
                    _catController.FlipX(true);
                    break;
                case < 0:
                    _catController.FlipX(false);
                    break;
            }
        }

        void PlayerIsDeath() {
            if (!IsOwner) return;
            if(_catObj == null || _catController == null) return;
            ChangeCatOwnershipRpc(_catObj, NetworkManager.Singleton.LocalClientId, false);
            ResetTheCatRpc();
            _catController?.SetPlayerCaptureElement(PlayerElement.None);
        }

        public void PickUpCat() {
            if (!IsOwner) return;
            if (_catPickUpArea && _catObj != null) {
                var value = GetComponent<SpriteRenderer>().flipX;
                var cat = _catController;
                cat.FlipX(!value);
                _catPickedUp = true;
                _catObjRb.bodyType = RigidbodyType2D.Kinematic;
                cat!.SetPlayerCaptureElement(_playerElement);
                ChangeCatOwnershipRpc(_catObj, NetworkManager.Singleton.LocalClientId, true);
            }
        }

        [Rpc(SendTo.Owner)]
        void ResetTheCatRpc() {
            if (_catController is not null)
                _catController.OnCatDisable -= ResetTheCatRpc;
            if (_catObjRb is not null)
                _catObjRb.bodyType = RigidbodyType2D.Dynamic;
            
            _catPickUpArea = false;
            _catPickedUp = false;
            _catObjRb = null;
            _catObj = null; 
            _catController = null;
        }

        void Update() {
            if (_catPickedUp && IsOwner && _catObj is not null && _catController is not null) {
                _catObj.transform.position = gameObject.transform.position + Vector3.up * CAT_IS_TALLER_THAN_PLAYER;
            }
        }

        public void ThrowCat() {
            if (_catPickedUp && IsOwner && _catObj != null && _catController != null) {
                _catObjRb.bodyType = RigidbodyType2D.Dynamic;
                var catToThrow = _catObj;
                var throwDirection = _spriteRenderer.flipX ? new Vector2(-1, 1) : Vector2.one;
                var throwVelocity = throwDirection * THROW_FORCE;
                
                PerformLocalThrow(catToThrow, throwVelocity);
                
                ThrowCatServerRpc(
                    catToThrow.GetComponent<NetworkObject>(),
                    throwVelocity
                );
                
                ResetTheCatRpc();
            }
        }

        [Rpc(SendTo.Server)]
        void ThrowCatServerRpc(NetworkObjectReference catNetRef, Vector2 throwVelocity) {
            if (catNetRef.TryGet(out NetworkObject catNetObj)) {
                _catObjRb = catNetObj.GetComponent<Rigidbody2D>();
                _catObjRb.linearVelocity = Vector2.zero;
                _catObjRb.AddForce(throwVelocity, ForceMode2D.Impulse);
                
                ThrowCatRpc(catNetRef, throwVelocity);
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        void ThrowCatRpc(NetworkObjectReference catNetRef, Vector2 throwVelocity) {
            if (IsOwner) return;
            
            if (catNetRef.TryGet(out NetworkObject catNetObj)) {
                PerformLocalThrow(catNetObj.gameObject, throwVelocity);
            }
        }

        void PerformLocalThrow(GameObject cat, Vector2 throwVelocity) {
            _catObjRb = cat.GetComponent<Rigidbody2D>();
            _catObjRb.linearVelocity = Vector2.zero;
            _catObjRb.AddForce(throwVelocity, ForceMode2D.Impulse);
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (!IsOwner) return;
            
            if (other.CompareTag(CAT_TAG) && other.gameObject.layer != gameObject.layer) {
                _catPickUpArea = true;
                _catObj = other.gameObject;
                _catObjRb = _catObj.GetComponent<Rigidbody2D>();
                _catController = _catObj.GetComponent<CatController>();
                _catController.OnCatDisable += ResetTheCatRpc;
            }
        }

        void OnTriggerExit2D(Collider2D other) {
            if (!IsOwner) return;
            
            if (other.CompareTag(CAT_TAG) && !_catPickedUp) {
                _catPickUpArea = false;
            }
        }

        [Rpc(SendTo.Server)]
        void ChangeCatOwnershipRpc(NetworkObjectReference other, ulong clientId, bool isEnter = false) {
            if (other.TryGet(out NetworkObject networkObject) && networkObject != null) {
                if(isEnter) {
                    if(networkObject.OwnerClientId != clientId) 
                        networkObject.ChangeOwnership(clientId);
                }
                else {
                    if(!networkObject.IsOwner) 
                        networkObject.RemoveOwnership();
                }
            }
        }

        public override void OnDestroy() {
            if (_healthController != null)
                _healthController.OnDeath -= PlayerIsDeath;
        }
    }
}