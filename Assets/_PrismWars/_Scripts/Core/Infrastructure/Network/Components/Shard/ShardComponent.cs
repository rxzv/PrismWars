using System;
using _PrismWars._Scripts.UI;
using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Infrastructure.Network.Components.Shard {
    public class ShardComponent : NetworkBehaviour, IInitializable<PlayerElement> {

        NetworkVariable<int> _countShards = new NetworkVariable<int>();
        
        PlayerElement _playerElement;
        GameUIViewService _uiGameViewService;
        HealthComponent _healthComponent;

        public void Initialize(PlayerElement playerElement) {
            _healthComponent = GetComponent<HealthComponent>();
            _healthComponent.OnDeath += DropAndClearShardsServerRpc;
            _healthComponent.OnDropShard += DropShardsServerRpc;
            _playerElement = playerElement;
            _uiGameViewService = ServiceLocator.Singleton.Get<GameUIViewService>();
            _countShards.OnValueChanged += ShardsCountChanged;
        }

        [ServerRpc(RequireOwnership = false)]
        void DropAndClearShardsServerRpc() {
            if (_countShards.Value > 0) {
                _countShards.Value = 0;
            }
            Debug.Log("Dropped all shards");
            // TODO: дропать все осколки
        }

        [ServerRpc(RequireOwnership = false)]
        void DropShardsServerRpc() {
            Debug.Log("Dropped shards");
            // TODO: дропать осколок
        }
        
        void ShardsCountChanged(int previousValue, int newValue) {
            if (newValue != previousValue) {
                _uiGameViewService.UpdateShardCount(newValue);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddShardServerRpc() {
            _countShards.Value++;
        }

    }
}