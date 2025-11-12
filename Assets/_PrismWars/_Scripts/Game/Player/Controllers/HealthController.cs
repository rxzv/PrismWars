using System;
using _PrismWars._Scripts.Components;
using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Health;
using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Services;
using _PrismWars._Scripts.Game.Services;
using _PrismWars._Scripts.UI;
using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;
using UnityEngine;

public class HealthController : NetworkBehaviour, IDamageable, IHeal, IInitializable<NetworkPlayerData> {
    float _maxHealth = 100f;
    NetworkVariable<float> _currentHealth = new(100f);
    
    NetworkVariable<PlayerElement> _currentPlayerElementDamaged = new();
    
    GameUIViewService _gameUIViewService;
    PlayerRespawnService _playerRespawnService;
    NetworkScoreService _networkScoreService;
    
    const int VALUE_TO_DROP_SHARD = 20;

    ulong _killerId;
    
    public event Action OnDeath;
    public event Action OnDropShard;
    
    public void Initialize(NetworkPlayerData data) {
        if (IsOwner) {
            _playerRespawnService = ServiceLocator.Singleton.Get<PlayerRespawnService>();
            _gameUIViewService = ServiceLocator.Singleton.Get<GameUIViewService>();
            _maxHealth = data.maxHealth;
            _gameUIViewService.SetMaxHealth(_maxHealth);
            _currentHealth.OnValueChanged += HealthChanged;
            _playerRespawnService.OnPlayerRespawn += PlayerRespawn;
            UpdateHealthServerRpc(data.maxHealth);
            _networkScoreService = ServiceLocator.Singleton.Get<NetworkScoreService>();
            
            _currentHealth.OnValueChanged += CheckDropShard;
        }
    }

    void CheckDropShard(float previousValue, float newValue) {
        if ((int)(newValue / VALUE_TO_DROP_SHARD) < (int)(previousValue / VALUE_TO_DROP_SHARD) && (int)newValue > 0) {
            OnDropShard?.Invoke();
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

    void HealthChanged(float oldHealth, float newHealth) {
        _gameUIViewService.UpdateHealthBar(newHealth);

        if (newHealth <= 0) {
            Debug.Log("Player died!");
            OnDeath?.Invoke();
            var networkObject = gameObject.GetComponent<NetworkObject>();
            _networkScoreService.AddScoreForDiedServerRpc(_currentPlayerElementDamaged.Value, 1, _killerId);
            _playerRespawnService.PlayerDeadServerRpc(NetworkManager.Singleton.LocalClientId, networkObject);
        }
    }
    public void TakeDamage(PlayerElement playerElement, float damage, ulong playerId) {
        _killerId = playerId;
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