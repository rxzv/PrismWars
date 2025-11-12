using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Services;
using _PrismWars._Scripts.Core.Patterns.Factory.ShardFactory;
using _PrismWars._Scripts.UI;
using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _PrismWars._Scripts.Game.Player.Controllers {
    public class ShardController : NetworkBehaviour, IInitializable<PlayerElement> {

        NetworkVariable<int> _countShards = new NetworkVariable<int>();
        
        PlayerElement _playerElement;
        GameUIViewService _uiGameViewService;
        HealthController _healthController;
        ShardFactory _shardFactory;

        public void Initialize(PlayerElement playerElement) {
            _healthController = GetComponent<HealthController>();
            _healthController.OnDeath += DropAndClearAllShards;
            _healthController.OnDropShard += DropShard;
            _playerElement = playerElement;
            _uiGameViewService = ServiceLocator.Singleton.Get<GameUIViewService>();
            _countShards.OnValueChanged += ShardsCountChanged;
        }

        void DropAndClearAllShards() {
            DropAllShardsServerRpc(transform.position, Vector3.up, _playerElement, _countShards.Value);
            DropAndClearShardsServerRpc();
        }
        
        [ServerRpc(RequireOwnership = false)]
        void DropAndClearShardsServerRpc() {
            if (_countShards.Value > 0) {
                _countShards.Value = 0;
            }
        }

        void DropShard() {
            DropShardsServerRpc(transform.position, Vector3.up, _playerElement);
        }

        [ServerRpc(RequireOwnership = false)]
        void DropShardsServerRpc(Vector3 position, Vector3 direction, PlayerElement playerElement) {
            Debug.Log("Dropped shards");
            _shardFactory = ServiceLocator.Singleton.Get<ShardFactory>();
            var randomPos = new Vector3(
                Random.Range(position.x - 1, position.x + 1),
                Random.Range(position.y, position.y + 0.5f),
                position.z
            );
            _shardFactory.Spawn(randomPos, direction, playerElement);
        }
        [ServerRpc(RequireOwnership = false)]
        void DropAllShardsServerRpc(Vector3 position, Vector3 direction, PlayerElement playerElement, int count) {
            Debug.Log("Dropped all shards");
            _shardFactory = ServiceLocator.Singleton.Get<ShardFactory>();
            for (int i = 0; i < count; i++) {
                var randomPos = new Vector3(
                    Random.Range(position.x - 1, position.x + 1),
                    Random.Range(position.y, position.y + 0.5f),
                    position.z
                );
                _shardFactory.Spawn(randomPos, direction, playerElement);
            }
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