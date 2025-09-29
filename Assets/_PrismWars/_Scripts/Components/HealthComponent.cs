using _PrismWars._Scripts.Components;
using _PrismWars._Scripts.Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealthComponent : NetworkBehaviour, IDamageable, IHeal
{
    [SerializeField] Slider _healthSlider;
    
    float _maxHealth = 100f;
    NetworkVariable<float> _currentHealth = new(100f);

    public override void OnNetworkSpawn() {
         _maxHealth = ServiceLocator.Current.Get<PlayerConfig>().MaxHealth;
        
        if (IsServer) 
            _currentHealth.Value = _maxHealth;
        
        _currentHealth.OnValueChanged += OnHealthChanged;
        UpdateHealthBar(_currentHealth.Value);
    }

    void OnHealthChanged(float oldHealth, float newHealth) {
        UpdateHealthBar(newHealth);
        
        if (newHealth <= 0)
            Debug.Log("Player died!");
    }

    void UpdateHealthBar(float health) {
        if (_healthSlider != null)
            _healthSlider.value = health / _maxHealth;
    }
    public void TakeDamage(float damage) {
        if (IsOwner)
            TakeDamageServerRpc(damage);
    }
    public void AddHealth(float heal) {
        if (IsOwner)
            AddHealthServerRpc(heal);
    }
    
    [ServerRpc(RequireOwnership = false)]
    void TakeDamageServerRpc(float damage) {
        if (_currentHealth.Value > 0)
            _currentHealth.Value = Mathf.Clamp(_currentHealth.Value - damage, 0, _maxHealth);
    }
    [ServerRpc(RequireOwnership = false)]
    void AddHealthServerRpc(float heal) {
        if (_currentHealth.Value > 0)
            _currentHealth.Value = Mathf.Clamp(_currentHealth.Value + heal, 0, _maxHealth);
    }

}