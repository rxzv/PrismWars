using System.Collections.Generic;
using _PrismWars._Scripts.Player;
using _PrismWars._Scripts.UI;
using _PrismWars._Scripts.UI.Model;
using _PrismWars._Scripts.Utils;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Game.Services {
    public class PlayerRespawnService : NetworkBehaviour, IService, IInitializable<List<Transform>, List<Transform>> {
        RespawnTimer _respawnTimer;
        Queue<NetworkObjectReference> _playerToRespawn = new();
        Queue<ulong> _playerToRespawnId = new();

        List<Transform> _fireSpawnPoints;
        List<Transform> _iceSpawnPoints;
        
        public void Initialize(List<Transform> fireSpawnPoints, List<Transform> iceSpawnPoints) {
            _fireSpawnPoints = fireSpawnPoints;
            _iceSpawnPoints = iceSpawnPoints;
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void PlayerDeadServerRpc(ulong clientId, NetworkObjectReference player) {
            PlayerDeadClientRpc(clientId, player);
            _playerToRespawn.Enqueue(player);
            _playerToRespawnId.Enqueue(clientId);
            PlayerStartRespawnTimerClientRpc(clientId);
        }

        [ClientRpc(RequireOwnership = false)]
        void PlayerStartRespawnTimerClientRpc(ulong clientId) {
            if (clientId == NetworkManager.Singleton.LocalClientId) {
                _respawnTimer = ServiceLocator.Singleton.Get<RespawnTimer>();
                _respawnTimer.OnTimerComplete += RespawnPlayerServerRpc;
                _respawnTimer.StartRespawnTimer(5f);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void RespawnPlayerServerRpc() {
            var pl = _playerToRespawn.Dequeue();
            var id = _playerToRespawnId.Dequeue();
            pl.TryGet(out NetworkObject networkObject);
            networkObject.TryGetComponent(out PlayerController playerController);
            if (playerController != null) {
                int spawnPointId;
                switch (playerController.PlayerElement.Value) {
                    default:
                    case PlayerElement.Fire:
                        spawnPointId = Random.Range(0, _fireSpawnPoints.Count);
                        playerController.transform.position = _fireSpawnPoints[spawnPointId].position;
                        break;
                    case PlayerElement.Ice:
                        spawnPointId = Random.Range(0, _iceSpawnPoints.Count);
                        playerController.transform.position = _iceSpawnPoints[spawnPointId].position;
                        break;
                }
            }
            PlayerRespawnClientRpc(id, pl);
        }

        [ClientRpc(RequireOwnership = false)]
        void PlayerRespawnClientRpc(ulong clientId, NetworkObjectReference player) {
            player.TryGet(out NetworkObject networkPlayer);
            if (networkPlayer != null) {
                networkPlayer.gameObject.SetActive(true);
                if (clientId == NetworkManager.Singleton.LocalClientId) {
                    networkPlayer.gameObject.GetComponent<PlayerController>().enabled = true;
                    ServiceLocator.Singleton.Get<NetworkUIManager>().OnPlayerRespawn();
                }
            } 
        }

        [ClientRpc(RequireOwnership = false)]
        void PlayerDeadClientRpc(ulong clientId, NetworkObjectReference player) {
            player.TryGet(out NetworkObject networkPlayer);
            if (networkPlayer != null) {
                networkPlayer.gameObject.SetActive(false);
                if (clientId == NetworkManager.Singleton.LocalClientId) {
                    networkPlayer.gameObject.GetComponent<PlayerController>().enabled = false;
                    ServiceLocator.Singleton.Get<NetworkUIManager>().OnPlayerDead();
                }
            }
        }

    }
}