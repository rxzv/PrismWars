using System;
using _PrismWars._Scripts.Components.Projectile;
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
        ShardFactory _shardFactory;

        public void Initialize(PlayerElement playerElement) {
            _healthComponent = GetComponent<HealthComponent>();
            _healthComponent.OnDeath += DropAndClearShardsServerRpc;
            _healthComponent.OnDropShard += DropShard;
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

        void DropShard() {
            DropShardsServerRpc(transform.position, Vector3.up, _playerElement);
        }

        [ServerRpc(RequireOwnership = false)]
        void DropShardsServerRpc(Vector3 position, Vector3 direction, PlayerElement playerElement) {
            Debug.Log("Dropped shards");
            // TODO: дропать осколок
            _shardFactory = ServiceLocator.Singleton.Get<ShardFactory>();
            _shardFactory.Spawn(position, direction, playerElement);
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