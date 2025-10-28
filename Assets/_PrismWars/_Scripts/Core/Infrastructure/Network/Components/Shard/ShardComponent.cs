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

        public void Initialize(PlayerElement playerElement) {
            _playerElement = playerElement;
            _uiGameViewService = ServiceLocator.Singleton.Get<GameUIViewService>();
            _countShards.OnValueChanged += ShardsCountChanged;
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