using System.Collections.Generic;
using _PrismWars._Scripts.Game.Player.Model;
using _PrismWars._Scripts.Player;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Infrastructure.Network.Components.BombCart {
    public class BombCartTriggerHandler : NetworkBehaviour {
        const string PLAYER_TAG = "Player";
        const string ZONE_TAG = "Zone";
        NetworkList<int> _playerIdsInTrigger = new();
        readonly Dictionary<ulong, PlayerElement> _playerElements = new();
        readonly List<GameObject> _zonesInTrigger = new();

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
            if (other.CompareTag(ZONE_TAG)) {
                _zonesInTrigger.Add(other.gameObject);
                OnZoneEntered(other.gameObject);
            }
        }

        void OnTriggerExit2D(Collider2D other) {
            if (!IsServer) return;

            if (other.CompareTag("Player") && other.TryGetComponent(out NetworkObject netObj)) {
                var playerId = (int)netObj.OwnerClientId;
                _playerIdsInTrigger.Remove(playerId);
                _playerElements.Remove(netObj.OwnerClientId);
            }

            if (other.CompareTag("Zone")) {
                _zonesInTrigger.Remove(other.gameObject);
            }
        }

        void OnZoneEntered(GameObject zoneObject) {
            var stateMachine = GetComponent<BombCartStateMachine>();
            var movement = GetComponent<BombCartMovement>();
            
            if (movement == null) return;

            // Определяем элемент зоны по Layout
            var zoneElement = GetZoneElement(zoneObject);
            
            // Тележка достигла вражеской базы (противоположной текущему целевому элементу)
            if (zoneElement != PlayerElement.None && zoneElement != movement.CurrentTargetElement)
            {
                stateMachine.SetState(stateMachine.InZoneState);
            }
        }

        PlayerElement GetZoneElement(GameObject zoneObject) {
            return zoneObject.layer.ToString() switch {
                "Fire" => PlayerElement.Fire,
                "Ice" => PlayerElement.Ice,
                _ => PlayerElement.None
            };
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