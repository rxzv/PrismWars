using _PrismWars._Scripts.Components;
using _PrismWars._Scripts.UI;
using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;
using UnityEngine;

public class HealthComponent : NetworkBehaviour, IDamageable, IHeal, IInitializable<NetworkPlayerData> {
    float _maxHealth = 100f;
    NetworkVariable<float> _currentHealth = new(100f);
    NetworkPlayerData _playerData;
    GameUIViewService _gameUIViewService;
    
    public void Initialize(NetworkPlayerData data) {
        if (IsOwner) {
            _gameUIViewService = ServiceLocator.Singleton.Get<GameUIViewService>();
            _playerData = data;
            _maxHealth = _playerData.maxHealth;
            _gameUIViewService.SetMaxHealth(_maxHealth);
            _currentHealth.OnValueChanged += OnHealthChanged;
        }
        if (IsServer) {
            _playerData = data;
            _currentHealth.Value = data.maxHealth;
        }
    }

    void OnHealthChanged(float oldHealth, float newHealth) {
        _gameUIViewService.UpdateHealthBar(newHealth);
        
        if (newHealth <= 0)
            Debug.Log("Player died!");
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