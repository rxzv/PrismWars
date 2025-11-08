using System;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Infrastructure.Network {
    public class CaptureTheCatComponent : MonoBehaviour {
        const string CAT_TAG = "Cat";
        const string ZONE_TAG = "Zone";
        
        bool _catPickUpArea = false;
        bool _catPickUp = false;
        
        GameObject _catObj;
        
        HealthComponent _healthComponent;

        void Start() {
            _healthComponent = GetComponent<HealthComponent>();
            _healthComponent!.OnDeath += PlayerIsDeath;
        }

        void PlayerIsDeath() {
            _catPickUpArea = false;
            _catObj = null;
        }

        public void PickUpCat() {
            if (_catPickUpArea) {
                _catPickUp = true;
            }
        }

        void Update() {
            if (_catPickUp) {
                _catObj.transform.position = _healthComponent.gameObject.transform.position + Vector3.up;
            }
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag(CAT_TAG) && other.gameObject.layer != gameObject.layer) {
                _catPickUpArea = true;
                _catObj = other.gameObject;
            }
        }
        void OnTriggerExit2D(Collider2D other) {
            if (other.CompareTag(CAT_TAG) && other.gameObject.layer != gameObject.layer) {
                _catPickUpArea = false;
                _catObj = null;
            }
        }

        void OnDisable() {
            _healthComponent!.OnDeath -= PlayerIsDeath;
        }

    }
}