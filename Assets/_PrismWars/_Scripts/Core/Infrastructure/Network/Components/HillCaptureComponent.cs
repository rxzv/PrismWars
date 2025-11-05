using System;
using System.Collections.Generic;
using _PrismWars._Scripts.Player;
using _PrismWars._Scripts.UI.Model;
using _PrismWars._Scripts.Utils;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Infrastructure.Network.Components {
    public class HillCaptureComponent : NetworkBehaviour {
        [SerializeField] float _captureDuration = 5f;
        
        NetworkVariable<PlayerElement> _hillElement = new NetworkVariable<PlayerElement>(PlayerElement.None);
        Dictionary<ulong, PlayerElement> _hillPlayers = new();
        
        NetworkVariable<int> _hillPlayersCount = new NetworkVariable<int>();
        Timer _hillCaptureTimer;
        SpriteRenderer _spriteRenderer;

        void OnTriggerEnter2D(Collider2D other) {
            if(!IsServer) return;
            if (other.TryGetComponent(out PlayerController player)) {
                _hillPlayers.Add(player.ClientId, player.PlayerElement.Value);
                _hillPlayersCount.Value = _hillPlayers.Count;
            }
        }
        void OnTriggerExit2D(Collider2D other) {
            if(!IsServer) return;
            if (other.TryGetComponent(out PlayerController player)) {
                _hillPlayers.Remove(player.ClientId);
                _hillPlayersCount.Value = _hillPlayers.Count;
            }
        }

        public override void OnNetworkSpawn() {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            UpdateHillColor(_hillElement.Value);
            
            if (IsServer) {
                _hillCaptureTimer = new Timer();
                _hillCaptureTimer.OnTimerComplete += OnCaptureComplete;
                _hillPlayersCount.OnValueChanged += HillPlayersCountChanged;
            }
            
            _hillElement.OnValueChanged += OnHillElementChanged;
            base.OnNetworkSpawn();
        }

        void HillPlayersCountChanged(int previousValue, int newValue) {
            if (newValue == 0) {
                _hillCaptureTimer.StopTimer();
                return;
            }

            PlayerElement dominantElement = GetDominantElement();
            
            if (dominantElement == PlayerElement.None) 
                _hillCaptureTimer.StopTimer();
            else 
                if (!IsTimerRunning())
                    _hillCaptureTimer.StartTimer(_captureDuration);
        }
        void OnCaptureComplete() {
            if (!IsServer) return;
            
            PlayerElement dominantElement = GetDominantElement();
            if (dominantElement != PlayerElement.None) 
                _hillElement.Value = dominantElement;
        }
        PlayerElement GetDominantElement() {
            if (_hillPlayers.Count == 0) return PlayerElement.None;

            PlayerElement? firstElement = null;
            foreach (var element in _hillPlayers.Values) {
                if (firstElement == null) 
                    firstElement = element;
                else if (firstElement != element)
                    return PlayerElement.None;
            }
            return firstElement.Value;
        }

        bool IsTimerRunning() => _hillCaptureTimer.GetRemainingTime() > 0;

        void Update() {
            if (!IsServer) return;
            _hillCaptureTimer?.Update();
        }

        void OnHillElementChanged(PlayerElement previous, PlayerElement current) {
            UpdateHillColor(current);
        }

        void UpdateHillColor(PlayerElement element) {
            if (_spriteRenderer != null)
                _spriteRenderer.color = GetColorHillByElement(element);
        }

        Color GetColorHillByElement(PlayerElement playerElement) =>
            playerElement switch
            {
                PlayerElement.None => Color.gray,
                PlayerElement.Ice => Color.blue,
                PlayerElement.Fire => Color.red,
                _ => Color.black
            };
    }
}