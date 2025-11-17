using System.Collections.Generic;
using _PrismWars._Scripts.Game.Player.Model;
using _PrismWars._Scripts.Player;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Infrastructure.Network.Components.BombCart {
    public class BombCartTriggerHandler : NetworkBehaviour {
        const string PLAYER_TAG = "Player";
        NetworkList<int> _playerIdsInTrigger = new();
        Dictionary<ulong, PlayerElement> _playerElements = new();

        public override void OnNetworkSpawn() {
            if (IsServer) {
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
            }
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (!IsServer) return;

            if (other.CompareTag(PLAYER_TAG) && other.TryGetComponent(out NetworkObject netObj)) {
                if (other.TryGetComponent(out PlayerController player)) {
                    var playerId = (int)netObj.OwnerClientId;
                    var playerElement = player.PlayerElement.Value;
                    
                    _playerIdsInTrigger.Add(playerId);
                    _playerElements[netObj.OwnerClientId] = playerElement;
                }
            }
        }

        void OnTriggerExit2D(Collider2D other) {
            if (!IsServer) return;

            if (other.CompareTag("Player") && other.TryGetComponent(out NetworkObject netObj)) {
                var playerId = (int)netObj.OwnerClientId;
                _playerIdsInTrigger.Remove(playerId);
                _playerElements.Remove(netObj.OwnerClientId);
            }
        }

        public PlayerElement GetDominantElement() {
            if (_playerIdsInTrigger.Count == 0) 
                return PlayerElement.None;

            PlayerElement? firstElement = null;
            
            foreach (var playerId in _playerIdsInTrigger) {
                var element = _playerElements[(ulong)playerId];
                
                if (firstElement == null) 
                    firstElement = element;
                else if (firstElement.Value != element)
                    return PlayerElement.None;
            }
            
            return firstElement!.Value;
        }

        void OnClientDisconnect(ulong clientId) {
            if (!IsServer) return;
            
            var playerId = (int)clientId;
            if (_playerIdsInTrigger.Contains(playerId)) {
                _playerIdsInTrigger.Remove(playerId);
                _playerElements.Remove(clientId);
            }
        }

        public override void OnNetworkDespawn() {
            if (IsServer && NetworkManager.Singleton != null) {
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
            }
            base.OnNetworkDespawn();
        }
    }
}