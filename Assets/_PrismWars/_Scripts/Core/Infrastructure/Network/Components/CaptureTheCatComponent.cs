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

        public event Action<bool> CatPickUp;

        void Start() {
            if(!IsOwner) return;
            _healthComponent = GetComponent<HealthComponent>();
            _healthComponent!.OnDeath += PlayerIsDeath;
        }

        void PlayerIsDeath() {
            if(!IsOwner) return;
            PlayerDiedServerRpc();
        }

        [ServerRpc]
        void PlayerDiedServerRpc() {
            _catPickUpArea = false;
            _catObj = null;
        }
        
        public void PickUpCat() {
            if(!IsOwner) return;
            PickUpCatServerRpc();
        }
        [ServerRpc]
         void PickUpCatServerRpc() {
            if (_catPickUpArea) {
                _catPickUped = true;
                CatIsPickedUpClientRpc(_catPickUped);
            }
         }

        [ClientRpc]
        void CatIsPickedUpClientRpc(bool isPickedUp) {
            if(IsOwner)
                CatPickUp?.Invoke(isPickedUp);
        }

        void Update() {
            if (_catPickUped && IsServer && _catObj) {
                _catObj.transform.position = gameObject.transform.position + Vector3.up;
            }
        }

        void OnTriggerEnter2D(Collider2D other) {
            if(!IsOwner) return;
            other.TryGetComponent(out NetworkObject networkObject);
            if(networkObject != null) 
                CatOnTriggerServerRpc(networkObject, true);
        }

        void OnTriggerExit2D(Collider2D other) {
            if(!IsOwner) return;
            other.TryGetComponent(out NetworkObject networkObject);
            if(networkObject != null) 
                CatOnTriggerServerRpc(networkObject, false);
        }

        [ServerRpc]
        void CatOnTriggerServerRpc(NetworkObjectReference other, bool isEnter = false) {
            other.TryGet(out NetworkObject cat);
            if (isEnter && cat != null) {
                if (cat.CompareTag(CAT_TAG) && cat.gameObject.layer != gameObject.layer) {
                    _catPickUpArea = true;
                    _catObj = cat.gameObject;
                }
            } else if (!isEnter && cat != null) {
                if (cat.CompareTag(CAT_TAG) && cat.gameObject.layer != gameObject.layer) {
                    _catPickUpArea = false;
                    _catObj = null;
                }
            }
        }

        void OnDisable() {
            _healthComponent!.OnDeath -= PlayerIsDeath;
        }

    }
}