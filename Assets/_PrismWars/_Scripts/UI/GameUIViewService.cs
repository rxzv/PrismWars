using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _PrismWars._Scripts.UI {
    public class GameUIViewService : MonoBehaviour, IService {
        [SerializeField] Slider _healthSlider;
        [SerializeField] TextMeshProUGUI _fireScore;
        [SerializeField] TextMeshProUGUI _iceScore;
        
        float _maxHealth;
        
        public void ShowView() => gameObject.SetActive(true);
        public void HideView() => gameObject.SetActive(false);
        
        public void SetMaxHealth(float maxHealth) {
            if (maxHealth > 0) {
                _maxHealth =  maxHealth;
                UpdateHealthBar(maxHealth);
            }
        }
        
        public void UpdateHealthBar(float health) {
            if (_healthSlider != null)
                _healthSlider.value = health / _maxHealth;
        }

        void Awake() {
            _fireScore.text = "0";
            _iceScore.text = "0";
        }

        void Update() {
            
        }

    }
}