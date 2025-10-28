using System;
using _PrismWars._Scripts.Game.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _PrismWars._Scripts.UI {
    public class GameUIViewService : MonoBehaviour, IService, IInitializable {
        [SerializeField] Slider _healthSlider;
        [SerializeField] TextMeshProUGUI _fireScore;
        [SerializeField] TextMeshProUGUI _iceScore;
        
        NetworkScoreManager _networkScoreManager;
        
        float _maxHealth;

        public void ShowView() => gameObject.SetActive(true);
        public void HideView() => gameObject.SetActive(false);
        

        public void Initialize() {
            _networkScoreManager = ServiceLocator.Singleton.Get<NetworkScoreManager>();
            _networkScoreManager.FireScore.OnValueChanged += FireScoreChanged;
            _networkScoreManager.IceScore.OnValueChanged += IceScoreChanged;
        }

        void FireScoreChanged(int previousValue, int newValue) {
            if (previousValue != newValue && newValue > 0) {
                _fireScore.text = newValue.ToString();
            }
        }
        void IceScoreChanged(int previousValue, int newValue) {
            if (previousValue != newValue && newValue > 0) {
                _iceScore.text = newValue.ToString();
            }
        }

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

    }
}