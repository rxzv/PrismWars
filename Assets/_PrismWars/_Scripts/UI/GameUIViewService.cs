using UnityEngine;
using UnityEngine.UI;

namespace _PrismWars._Scripts.UI {
    public class GameUIViewService : MonoBehaviour, IService {
        [SerializeField] Slider _healthSlider;
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
    }
}