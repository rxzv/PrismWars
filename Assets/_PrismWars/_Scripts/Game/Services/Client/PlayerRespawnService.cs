using System;
using System.Collections.Generic;
using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Services;
using _PrismWars._Scripts.Game.Player.Model;
using _PrismWars._Scripts.Game.Services.Mono;
using _PrismWars._Scripts.Player;
using _PrismWars._Scripts.UI;
using _PrismWars._Scripts.Utils;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _PrismWars._Scripts.Game.Services.Client {
    public class PlayerRespawnService : NetworkBehaviour, IService, IInitializable<Vector3[], Vector3[]> {
        const float PLAYER_TIME_TO_RESPAWN = 5f;
        Timer _respawnTimer;
        Queue<NetworkObjectReference> _playerToRespawn = new();
        Queue<ulong> _playerToRespawnId = new();

        Vector3[] _fireSpawnPoints;
        Vector3[] _iceSpawnPoints;
        
        public event Action OnPlayerRespawn;
        
        public void Initialize(Vector3[] fireSpawnPoints, Vector3[] iceSpawnPoints) {
            _fireSpawnPoints = fireSpawnPoints;
            _iceSpawnPoints = iceSpawnPoints;
            _respawnTimer = new Timer();
            ServiceLocator.Singleton.Get<ClientUIManager>().SetRespawnTimer(_respawnTimer);
        }
        
        [Rpc(SendTo.Server)]
        public void PlayerDeadServerRpc(ulong clientId, NetworkObjectReference player) {
            PlayerDeadClientRpc(clientId, player);
            _playerToRespawn.Enqueue(player);
            _playerToRespawnId.Enqueue(clientId);
            PlayerStartRespawnTimerClientRpc(clientId);
        }

        [Rpc(SendTo.ClientsAndHost)]
        void PlayerStartRespawnTimerClientRpc(ulong clientId) {
            if (clientId == NetworkManager.Singleton.LocalClientId) {
                _respawnTimer.OnTimerComplete += RespawnPlayerServerRpc;
                _respawnTimer.StartTimer(PLAYER_TIME_TO_RESPAWN);
            }
        }

        void Update() {
            _respawnTimer?.Update();
        }

        [Rpc(SendTo.Server)]
        void RespawnPlayerServerRpc() {
            var pl = _playerToRespawn.Dequeue();
            var id = _playerToRespawnId.Dequeue();
            pl.TryGet(out NetworkObject networkObject);
            networkObject.TryGetComponent(out PlayerController playerController);
            if (playerController is not null) {
                int spawnPointId;
                Vector3 spawnPos;
                switch (playerController.PlayerElement.Value) {
                    default:
                    case PlayerElement.Fire:
                        spawnPointId = Random.Range(0, _fireSpawnPoints.Length - 1);
                        spawnPos = _fireSpawnPoints[spawnPointId];
                        PlayerUpdateSpawnPositionClientRpc(id, spawnPos, networkObject);
                        break;
                    case PlayerElement.Ice:
                        spawnPointId = Random.Range(0, _iceSpawnPoints.Length - 1);
                        spawnPos = _iceSpawnPoints[spawnPointId];
                        PlayerUpdateSpawnPositionClientRpc(id, spawnPos, networkObject);
                        break;
                }
            }
            PlayerRespawnClientRpc(id, pl);
        }

        [Rpc(SendTo.ClientsAndHost)]
        void PlayerUpdateSpawnPositionClientRpc(ulong clientId, Vector3 spawnPos, NetworkObjectReference player) {
            if(clientId != NetworkManager.Singleton.LocalClientId) return;
            player.TryGet(out NetworkObject networkObject);
            if(networkObject is null) return;
            networkObject.TryGetComponent(out PlayerController playerController);
            playerController.PlayerSetRespawnPositionRpc(spawnPos);
        }

        [Rpc(SendTo.ClientsAndHost)]
        void PlayerRespawnClientRpc(ulong clientId, NetworkObjectReference player) {
            player.TryGet(out NetworkObject networkPlayer);
            if(networkPlayer is null) return;
            if(clientId != NetworkManager.Singleton.LocalClientId) return;
            OnPlayerRespawn?.Invoke();
            ServiceLocator.Singleton.Get<ClientUIManager>().OnPlayerRespawn();
            ServiceLocator.Singleton.Get<InputService>().InputActionEnable();
            networkPlayer.gameObject.TryGetComponent(out PlayerController playerController);
            if(playerController is null) return;
            playerController.enabled = true;
            playerController.PlayerShowRpc();
        }

        [Rpc(SendTo.ClientsAndHost)]
        void PlayerDeadClientRpc(ulong clientId, NetworkObjectReference player) {
            player.TryGet(out NetworkObject networkPlayer);
            if (networkPlayer != null) {
                networkPlayer.gameObject.SetActive(false);
                if (clientId == NetworkManager.Singleton.LocalClientId) {
                    networkPlayer.gameObject.GetComponent<PlayerController>().enabled = false;
                    ServiceLocator.Singleton.Get<ClientUIManager>().OnPlayerDead();
                    ServiceLocator.Singleton.Get<InputService>().InputActionDisable();
                }
            }
        }

    }
}