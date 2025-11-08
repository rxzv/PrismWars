using System;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Infrastructure.Network {
    public class CaptureTheCatComponent : NetworkBehaviour {

        const string CAT_TAG = "Cat";
        const string ZONE_TAG = "Zone";

        bool _catPickUpArea = false;
        bool _catPickedUp = false;

        GameObject _catObj;

        HealthComponent _healthComponent;
        public bool CatPickedUp => _catPickedUp;

        void Start() {
            if (!IsOwner) return;
            _healthComponent = GetComponent<HealthComponent>();
            _healthComponent!.OnDeath += PlayerIsDeath;
        }

        void PlayerIsDeath() {
            if (!IsOwner) return;
            ChangeCatOwnershipServerRpc(_catObj, NetworkManager.Singleton.LocalClientId, false);
            _catPickUpArea = false;
            _catPickedUp = false;
            _catObj = null;
        }

        public void PickUpCat() {
            if (!IsOwner) return;
            if (_catPickUpArea) {
                _catPickedUp = true;
                ChangeCatOwnershipServerRpc(_catObj, NetworkManager.Singleton.LocalClientId, true);
            }
        }

        void Update() {
            if (_catPickedUp && IsOwner && _catObj) {
                _catObj.transform.position = gameObject.transform.position + Vector3.up * 1.3f;
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
            else {
                other.TryGet(out NetworkObject networkObject);
                networkObject.ChangeOwnership(NetworkManager.Singleton.LocalClientId);
            }
        }
        
        void OnDestroy() {
            _healthComponent!.OnDeath -= PlayerIsDeath;
        }

    }
}