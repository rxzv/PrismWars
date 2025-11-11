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
                    _catObj.GetComponent<CatController>().FlipX(true);
                else if(newDir < 0)
                    _catObj.GetComponent<CatController>().FlipX(false);
            }
        }

        void PlayerIsDeath() {
            if (!IsOwner) return;
            if (_catObj != null) {
                ChangeCatOwnershipRpc(_catObj, NetworkManager.Singleton.LocalClientId, false);
                ResetTheCat();
                _catObj.GetComponent<CatController>()?.SetPlayerCaptureElementRpc(PlayerElement.None);
            }
        }

        public void PickUpCat() {
            if (!IsOwner) return;
            if (_catPickUpArea && _catObj != null) {
                var value = GetComponent<SpriteRenderer>().flipX;
                var cat = _catObj.GetComponent<CatController>();
                cat.FlipX(!value);
                _catPickedUp = true;
                _catObj!.GetComponent<Rigidbody2D>().isKinematic = true;
                cat!.SetPlayerCaptureElementRpc(_playerElement);
                ChangeCatOwnershipRpc(_catObj, NetworkManager.Singleton.LocalClientId, true);
            }
        }

        void ResetTheCat() {
            _catObj.GetComponent<CatController>().OnCatDisable -= ResetTheCat;
            _catObj!.GetComponent<Rigidbody2D>().isKinematic = false;
            _catPickUpArea = false;
            _catPickedUp = false;
            _catObj = null; 
        }

        void Update() {
            if (_catPickedUp && IsOwner && _catObj != null) {
                _catObj.transform.position = gameObject.transform.position + Vector3.up * CAT_UP_TO_PLAYER;
            }
        }

        public void ThrowCat(bool isFlipX) {
            if (_catPickedUp && IsOwner && _catObj != null) {
                _catObj!.GetComponent<Rigidbody2D>().isKinematic = false;
                GameObject catToThrow = _catObj;
                Vector2 throwDirection = isFlipX ? new Vector2(-1, 1) : Vector2.one;
                Vector2 throwVelocity = throwDirection * THROW_FORCE;
                
                PerformLocalThrow(catToThrow, throwVelocity);
                
                ThrowCatServerRpc(
                    catToThrow.GetComponent<NetworkObject>(),
                    throwVelocity,
                    NetworkManager.Singleton.LocalClientId
                );
                
                ResetTheCat();
            }
        }

        [Rpc(SendTo.Server)]
        void ThrowCatServerRpc(NetworkObjectReference catNetRef, Vector2 throwVelocity, ulong clientId) {
            if (catNetRef.TryGet(out NetworkObject catNetObj)) {
                Rigidbody2D catRb = catNetObj.GetComponent<Rigidbody2D>();
                catRb.linearVelocity = Vector2.zero;
                catRb.AddForce(throwVelocity, ForceMode2D.Impulse);
                
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
            Rigidbody2D catRb = cat.GetComponent<Rigidbody2D>();
            catRb.linearVelocity = Vector2.zero;
            catRb.AddForce(throwVelocity, ForceMode2D.Impulse);
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (!IsOwner) return;
            
            if (other.CompareTag(CAT_TAG) && other.gameObject.layer != gameObject.layer) {
                _catPickUpArea = true;
                _catObj = other.gameObject;
                _catObj.GetComponent<CatController>().OnCatDisable += ResetTheCat;
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