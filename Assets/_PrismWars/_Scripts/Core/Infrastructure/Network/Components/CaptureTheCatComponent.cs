using System;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Infrastructure.Network {
    public class CaptureTheCatComponent : NetworkBehaviour {

        const string CAT_TAG = "Cat";
        const string ZONE_TAG = "Zone";

        bool _catPickUpArea = false;
        bool _catPickUped = false;

        GameObject _catObj;

        HealthComponent _healthComponent;

        void Start() {
            if (!IsOwner) return;
            _healthComponent = GetComponent<HealthComponent>();
            _healthComponent!.OnDeath += PlayerIsDeath;
        }

        void PlayerIsDeath() {
            if (!IsOwner) return;
            _catPickUpArea = false;
            _catObj = null;
        }

        public void PickUpCat() {
            if (!IsOwner) return;
            if (_catPickUpArea) {
                _catPickUped =  true;
                ChangeOwnershipServerRpc(_catObj, NetworkManager.Singleton.LocalClientId, true);
            }
        }

        void Update() {
            if (_catPickUped && IsOwner && _catObj) {
                _catObj.transform.position = gameObject.transform.position + Vector3.up;
            }
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (!IsOwner) return;
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
            if (networkObject != null && !_catPickUped) {
                _catPickUpArea = false;
                _catObj = null;
            }
        }

        [ServerRpc]
        void ChangeOwnershipServerRpc(NetworkObjectReference other, ulong clientId, bool isEnter = false) {
            if (isEnter) {
                other.TryGet(out NetworkObject networkObject);
                networkObject.ChangeOwnership(clientId);
            }
            else {
                other.TryGet(out NetworkObject networkObject);
                networkObject.ChangeOwnership(NetworkManager.Singleton.LocalClientId);
            }
        }
        
        void OnDisable() {
            _healthComponent!.OnDeath -= PlayerIsDeath;
        }

    }
}