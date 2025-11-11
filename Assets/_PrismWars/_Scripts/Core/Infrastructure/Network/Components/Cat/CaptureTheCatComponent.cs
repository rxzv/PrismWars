using _PrismWars._Scripts.Core.Infrastructure.Network.Components;
using _PrismWars._Scripts.Player;
using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Infrastructure.Network {
    public class CaptureTheCatComponent : NetworkBehaviour, IInitializable {
        const float THROW_FORCE = 100f;
        const string CAT_TAG = "Cat";
        const float CAT_UP_TO_PLAYER = 1.3f;

        bool _catPickUpArea = false;
        bool _catPickedUp = false;

        GameObject _catObj;
        Rigidbody2D _catObjRb;
        CatController _catController;
        
        HealthComponent _healthComponent;
        PlayerElement _playerElement;
        
        public bool CatPickedUp => _catPickedUp;

        public void Initialize() {
            if (!IsOwner) return;
            _healthComponent = GetComponent<HealthComponent>();
            _playerElement = GetComponent<PlayerController>().PlayerElement.Value;
            _healthComponent!.OnDeath += PlayerIsDeath;
        }

        public void FlipXCat(float oldDir, float newDir) {
            if(_catPickedUp && IsOwner && _catObj != null) {
                if(newDir > 0)
                    _catController.FlipX(true);
                else if(newDir < 0)
                    _catController.FlipX(false);
            }
        }

        void PlayerIsDeath() {
            if (!IsOwner) return;
            if (_catObj != null) {
                ChangeCatOwnershipRpc(_catObj, NetworkManager.Singleton.LocalClientId, false);
                ResetTheCatRpc();
                _catController?.SetPlayerCaptureElementRpc(PlayerElement.None);
            }
        }

        public void PickUpCat() {
            if (!IsOwner) return;
            if (_catPickUpArea && _catObj != null) {
                var value = GetComponent<SpriteRenderer>().flipX;
                var cat = _catController;
                cat.FlipX(!value);
                _catPickedUp = true;
                _catObjRb.bodyType = RigidbodyType2D.Kinematic;
                cat!.SetPlayerCaptureElementRpc(_playerElement);
                ChangeCatOwnershipRpc(_catObj, NetworkManager.Singleton.LocalClientId, true);
            }
        }

        [Rpc(SendTo.Owner)]
        void ResetTheCatRpc() {
            _catController.OnCatDisable -= ResetTheCatRpc;
            _catObjRb.bodyType = RigidbodyType2D.Dynamic;
            _catPickUpArea = false;
            _catPickedUp = false;
            _catObjRb = null;
            _catObj = null; 
            _catController = null;
        }

        void Update() {
            if (_catPickedUp && IsOwner && _catObj != null) {
                _catObj.transform.position = gameObject.transform.position + Vector3.up * CAT_UP_TO_PLAYER;
            }
        }

        public void ThrowCat(bool isFlipX) {
            if (_catPickedUp && IsOwner && _catObj != null) {
                _catObjRb.bodyType = RigidbodyType2D.Dynamic;
                GameObject catToThrow = _catObj;
                Vector2 throwDirection = isFlipX ? new Vector2(-1, 1) : Vector2.one;
                Vector2 throwVelocity = throwDirection * THROW_FORCE;
                
                PerformLocalThrow(catToThrow, throwVelocity);
                
                ThrowCatServerRpc(
                    catToThrow.GetComponent<NetworkObject>(),
                    throwVelocity,
                    NetworkManager.Singleton.LocalClientId
                );
                
                ResetTheCatRpc();
            }
        }

        [Rpc(SendTo.Server)]
        void ThrowCatServerRpc(NetworkObjectReference catNetRef, Vector2 throwVelocity, ulong clientId) {
            if (catNetRef.TryGet(out NetworkObject catNetObj)) {
                _catObjRb = catNetObj.GetComponent<Rigidbody2D>();
                _catObjRb.linearVelocity = Vector2.zero;
                _catObjRb.AddForce(throwVelocity, ForceMode2D.Impulse);
                
                ThrowCatRpc(catNetRef, throwVelocity, clientId);
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        void ThrowCatRpc(NetworkObjectReference catNetRef, Vector2 throwVelocity, ulong clientId) {
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
                if (_catObj != null) {
                    ChangeCatOwnershipRpc(_catObj, NetworkManager.Singleton.LocalClientId, false);
                }
            }
        }

        [Rpc(SendTo.Server)]
        void ChangeCatOwnershipRpc(NetworkObjectReference other, ulong clientId, bool isEnter = false) {
            if (other.TryGet(out NetworkObject networkObject) && networkObject != null) {
                if (isEnter) 
                    networkObject.ChangeOwnership(clientId);
                else 
                    networkObject.RemoveOwnership();
            }
        }
        
        void OnDestroy() {
            if (_healthComponent != null)
                _healthComponent.OnDeath -= PlayerIsDeath;
        }
    }
}