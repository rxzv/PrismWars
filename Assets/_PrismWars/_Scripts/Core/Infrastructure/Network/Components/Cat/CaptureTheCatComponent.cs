using _PrismWars._Scripts.Core.Infrastructure.Network.Components;
using _PrismWars._Scripts.Player;
using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Infrastructure.Network {
    public class CaptureTheCatComponent : NetworkBehaviour, IInitializable {
        const float THROW_FORCE = 10f;
        const string CAT_TAG = "Cat";

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

        void PlayerIsDeath() {
            if (!IsOwner) return;
            if (_catObj != null)
            {
                ChangeCatOwnershipServerRpc(_catObj, NetworkManager.Singleton.LocalClientId, false);
                ResetTheCat();
                _catObj.GetComponent<CatController>()?.SetPlayerCaptureElementServerRpc(PlayerElement.None);
            }
        }

        public void PickUpCat() {
            if (!IsOwner) return;
            if (_catPickUpArea && _catObj != null) {
                _catPickedUp = true;
                _catObj.GetComponent<CatController>()!.SetPlayerCaptureElementServerRpc(_playerElement);
                ChangeCatOwnershipServerRpc(_catObj, NetworkManager.Singleton.LocalClientId, true);
            }
        }

        void ResetTheCat() {
            _catPickUpArea = false;
            _catPickedUp = false;
            _catObj = null; 
        }

        void Update() {
            if (_catPickedUp && IsOwner && _catObj != null) {
                _catObj.transform.position = gameObject.transform.position + Vector3.up * 1.3f;
            }
        }

        public void ThrowCat(bool isFlipX) {
            if (_catPickedUp && IsOwner && _catObj != null) {
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

        [ServerRpc]
        void ThrowCatServerRpc(NetworkObjectReference catNetRef, Vector2 throwVelocity, ulong clientId) {
            if (catNetRef.TryGet(out NetworkObject catNetObj)) {
                Rigidbody2D catRb = catNetObj.GetComponent<Rigidbody2D>();
                catRb.linearVelocity = Vector2.zero;
                catRb.AddForce(throwVelocity, ForceMode2D.Impulse);
                
                ThrowCatClientRpc(catNetRef, throwVelocity, clientId);
            }
        }

        [ClientRpc(RequireOwnership = false)]
        void ThrowCatClientRpc(NetworkObjectReference catNetRef, Vector2 throwVelocity, ulong clientId) {
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
            }
        }

        void OnTriggerExit2D(Collider2D other) {
            if (!IsOwner) return;
            
            if (other.CompareTag(CAT_TAG) && !_catPickedUp) {
                _catPickUpArea = false;
                if (_catObj != null)
                {
                    ChangeCatOwnershipServerRpc(_catObj, NetworkManager.Singleton.LocalClientId, false);
                }
            }
        }

        [ServerRpc]
        void ChangeCatOwnershipServerRpc(NetworkObjectReference other, ulong clientId, bool isEnter = false) {
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