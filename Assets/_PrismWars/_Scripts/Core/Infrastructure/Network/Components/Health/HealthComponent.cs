using _PrismWars._Scripts.Components;
using _PrismWars._Scripts.Game.Services;
using _PrismWars._Scripts.UI;
using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;
using UnityEngine;

public class HealthComponent : NetworkBehaviour, IDamageable, IHeal, IInitializable<NetworkPlayerData> {
    float _maxHealth = 100f;
    NetworkVariable<float> _currentHealth = new(100f);
    
    NetworkVariable<PlayerElement> _currentPlayerElementDamaged = new();
    
    NetworkPlayerData _playerData;
    GameUIViewService _gameUIViewService;
    PlayerRespawnService _playerRespawnService;
    NetworkScoreManager _networkScoreManager;
    
    public void Initialize(NetworkPlayerData data) {
        if (IsOwner) {
            _playerRespawnService = ServiceLocator.Singleton.Get<PlayerRespawnService>();
            _gameUIViewService = ServiceLocator.Singleton.Get<GameUIViewService>();
            _playerData = data;
            _maxHealth = data.maxHealth;
            _gameUIViewService.SetMaxHealth(_maxHealth);
            _currentHealth.OnValueChanged += OnHealthChanged;
            _playerRespawnService.OnPlayerRespawn += PlayerRespawn;
            UpdateHealthServerRpc(data.maxHealth);
            _networkScoreManager = ServiceLocator.Singleton.Get<NetworkScoreManager>();
        }
    }

    void PlayerRespawn() {
        if (IsOwner) {
            UpdateHealthServerRpc(_maxHealth);
        }
    }

    [ServerRpc]
    void UpdateHealthServerRpc(float maxHealth) {
        if (maxHealth > 0) {
            _currentHealth.Value = maxHealth;
        }
    }

    void OnHealthChanged(float oldHealth, float newHealth) {
        _gameUIViewService.UpdateHealthBar(newHealth);

        if (newHealth <= 0) {
            Debug.Log("Player died!");
            var networkObject = gameObject.GetComponent<NetworkObject>();
            _networkScoreManager.AddScoreServerRpc(_currentPlayerElementDamaged.Value, 10);
            _playerRespawnService.PlayerDeadServerRpc(NetworkManager.Singleton.LocalClientId, networkObject);
        }
    }
    public void TakeDamage(PlayerElement playerElement, float damage) {
        TakeDamageServerRpc(playerElement, damage);
    }
    public void AddHealth(float heal) {
        AddHealthServerRpc(heal);
    }
    
    [ServerRpc(RequireOwnership = false)]
    void TakeDamageServerRpc(PlayerElement playerElement, float damage) {
        if (_currentHealth.Value > 0) {
            _currentHealth.Value = Mathf.Clamp(_currentHealth.Value - damage, 0, _maxHealth);

            _currentPlayerElementDamaged.Value = playerElement;
        }
    }
    [ServerRpc(RequireOwnership = false)]
    void AddHealthServerRpc(float heal) {
        if (_currentHealth.Value > 0)
            _currentHealth.Value = Mathf.Clamp(_currentHealth.Value + heal, 0, _maxHealth);
    }

}