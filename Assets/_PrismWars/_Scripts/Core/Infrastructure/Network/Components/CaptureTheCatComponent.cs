using _PrismWars._Scripts.Core.Infrastructure.Network.Components;
using _PrismWars._Scripts.Player;
using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Infrastructure.Network {
    public class CaptureTheCatComponent : NetworkBehaviour {
        const float THROW_FORCE = 10f;
        const string CAT_TAG = "Cat";
        const string ZONE_TAG = "Zone";

        bool _catPickUpArea = false;
        bool _catPickedUp = false;

        GameObject _catObj;

        HealthComponent _healthComponent;
        PlayerElement _playerElement;
        public bool CatPickedUp => _catPickedUp;

        void Start() {
            if (!IsOwner) return;
            _healthComponent = GetComponent<HealthComponent>();
            _playerElement = GetComponent<PlayerController>().PlayerElement.Value;
            _healthComponent!.OnDeath += PlayerIsDeath;
        }

        void PlayerIsDeath() {
            if (!IsOwner) return;
            ChangeCatOwnershipServerRpc(_catObj, NetworkManager.Singleton.LocalClientId, false);
            ResetTheCat();
        }

        public void PickUpCat() {
            if (!IsOwner) return;
            if (_catPickUpArea) {
                _catPickedUp = true;
                _catObj.GetComponent<CatController>()?.SetPlayerCaptureElementServerRpc(_playerElement);
                ChangeCatOwnershipServerRpc(_catObj, NetworkManager.Singleton.LocalClientId, true);
            }
        }

        void ResetTheCat() {
            _catObj.GetComponent<CatController>()?.SetPlayerCaptureElementServerRpc(PlayerElement.None);
            _catPickUpArea = false;
            _catPickedUp = false;
            _catObj = null;
        }

        void Update() {
            if (_catPickedUp && IsOwner && _catObj) {
                _catObj.transform.position = gameObject.transform.position + Vector3.up * 1.3f;
            }
        }

        public void ThrowCat(bool isFlipX) {
            if (_catPickedUp && IsOwner && _catObj) {
                Vector2 throwDirection = isFlipX ? new Vector2(-1, 1) : Vector2.one;
                _catObj.GetComponent<Rigidbody2D>().AddForce(throwDirection * THROW_FORCE, ForceMode2D.Impulse);
                ChangeCatOwnershipServerRpc(_catObj, NetworkManager.Singleton.LocalClientId, false);
                ResetTheCat();
            }
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (!IsOwner && _catObj) return;
            other.TryGetComponent(out NetworkObject networkObject);
            if (networkObject != null) {
                if (networkObject.CompareTag(CAT_TAG) && networkObject.gameObject.layer != gameObject.layer) {
                    _catPickUpArea = true;
                    _catObj = networkObject.gameObject;
                }
            }
        }

        void OnTriggerExit2D(Collider2D other) {
            if (!IsOwner) return;
            other.TryGetComponent(out NetworkObject networkObject);
            if (networkObject != null && !_catPickedUp) {
                _catPickUpArea = false;
                _catObj = null;
            }
        }

        [ServerRpc]
        void ChangeCatOwnershipServerRpc(NetworkObjectReference other, ulong clientId, bool isEnter = false) {
            if (isEnter) {
                other.TryGet(out NetworkObject networkObject);
                networkObject.ChangeOwnership(clientId);
            }
            else 
                ChangeCatOwnershipRpc(other, clientId);
        }

        [Rpc(SendTo.Owner)]
        void ChangeCatOwnershipRpc(NetworkObjectReference other, ulong clientId) {
            other.TryGet(out NetworkObject networkObject);
            networkObject.ChangeOwnership(clientId);
        }
        
        void OnDestroy() {
            _healthComponent!.OnDeath -= PlayerIsDeath;
        }

    }
}