using System.Collections;
using _PrismWars._Scripts.Components;
using _PrismWars._Scripts.Player;
using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealthComponent : NetworkBehaviour, IDamageable, IHeal, IInitializable<PlayerConfig>
{
    [SerializeField] Slider _healthSlider;
    
    float _maxHealth = 100f;
    NetworkVariable<float> _currentHealth = new(100f);
    PlayerConfig _config;

    bool _isInitialized = false;

    public void Initialize(PlayerConfig config) {
        _config = config;
        _maxHealth = _config.maxHealth;
        _isInitialized = true;
    }
    IEnumerator WaitForInitialization() {
        while (!_isInitialized || !_config)
            yield return null;
        Debug.Log("Config initialized: HealthComponent");
    }
    public override void OnNetworkSpawn() {
        StartCoroutine(WaitForInitialization());
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
        TakeDamageServerRpc(damage);
    }
    public void AddHealth(float heal) {
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