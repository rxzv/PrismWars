using System;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Infrastructure.Network {
    public class CaptureTheCatComponent : MonoBehaviour {
        const string CAT_TAG = "Cat";
        const string ZONE_TAG = "Zone";
        
        bool _catPickUpArea = false;

        public bool CatPickUpArea => _catPickUpArea;
        
        HealthComponent _healthComponent;

        void Start() {
            _healthComponent = GetComponent<HealthComponent>();
            _healthComponent!.OnDeath += PlayerIsDeath;
        }

        void PlayerIsDeath() {
            _catPickUpArea = false;
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag(CAT_TAG) && other.gameObject.layer != gameObject.layer) {
                _catPickUpArea = true;
            }
        }
        void OnTriggerExit2D(Collider2D other) {
            if (other.CompareTag(CAT_TAG) && other.gameObject.layer != gameObject.layer) {
                _catPickUpArea = false;
            }
        }

        void OnDisable() {
            _healthComponent!.OnDeath -= PlayerIsDeath;
        }

    }
}