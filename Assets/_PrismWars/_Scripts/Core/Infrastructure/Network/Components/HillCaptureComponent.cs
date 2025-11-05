using _PrismWars._Scripts.Player;
using _PrismWars._Scripts.UI.Model;
using _PrismWars._Scripts.Utils;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Infrastructure.Network.Components {
    public class HillCaptureComponent : NetworkBehaviour {
        [SerializeField] float _captureDuration = 5f;
        
        NetworkVariable<PlayerElement> _hillElement = new ();
        NetworkList<int> _hillPlayers = new();
        
        Timer _hillCaptureTimer;
        SpriteRenderer _spriteRenderer;
        Color _startColor;
        Color _targetColor;
        float _captureStartTime;
        bool _isCapturing;

        public override void OnNetworkSpawn() {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            UpdateHillColor(_hillElement.Value);
            
            if (IsServer) {
                _hillCaptureTimer = new Timer();
                _hillCaptureTimer.OnTimerComplete += OnCaptureComplete;
                _hillPlayers.OnListChanged += HillPlayersCountChanged;
            }
            
            _hillElement.OnValueChanged += OnHillElementChanged;
            base.OnNetworkSpawn();
        }

        void HillPlayersCountChanged(NetworkListEvent<int> changeEvent) {
            if (!IsServer) return;
            if (changeEvent.Value == 0) {
                _hillCaptureTimer.StopTimer();
                _isCapturing = false;
                return;
            }

            PlayerElement dominantElement = GetDominantElement();

            if (dominantElement == PlayerElement.None) {
                _hillElement.Value = dominantElement;
                if (IsTimerRunning()) {
                    _hillCaptureTimer.StopTimer();
                    _isCapturing = false;
                }
            }
            else if (!IsTimerRunning() && _hillElement.Value != dominantElement) {
                _hillCaptureTimer.StartTimer(_captureDuration);
                _isCapturing = true;
                _captureStartTime = Time.time;
                _startColor = GetColorHillByElement(_hillElement.Value);
                _targetColor = GetColorHillByElement(dominantElement);
            }
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (!IsServer) return;
            if (other.TryGetComponent(out PlayerController player)) {
                _hillPlayers.Add(GetPlayerIntForPlayerElement(player.PlayerElement.Value));
            }
        }

        int GetPlayerIntForPlayerElement(PlayerElement playerElement) {
            return playerElement switch {
                PlayerElement.None => 0,
                PlayerElement.Fire => 1,
                PlayerElement.Ice => 2,
                _ => -1
            };
        }
        PlayerElement GetPlayerElementForId(int id) {
            return id switch {
                0 => PlayerElement.None,
                1 => PlayerElement.Fire,
                2 => PlayerElement.Ice,
                _ => PlayerElement.None
            };
        }
        
        void OnTriggerExit2D(Collider2D other) {
            if(!IsServer) return;
            if (other.TryGetComponent(out PlayerController player)) {
                var id = GetPlayerIntForPlayerElement(player.PlayerElement.Value);
                _hillPlayers.Remove(id);
            }
            Debug.Log("Exit");
        }

        void OnCaptureComplete() {
            if (!IsServer) return;
            Debug.Log("OnCaptureComplete");
            
            PlayerElement dominantElement = GetDominantElement();
            if (dominantElement != PlayerElement.None) 
                _hillElement.Value = dominantElement;
        }
        PlayerElement GetDominantElement() {
            if (_hillPlayers.Count == 0) 
                return PlayerElement.None;

            int? firstElement = null;
    
            foreach (var element in _hillPlayers) {
                if (firstElement == null) {
                    firstElement = element;
                }
                else if (firstElement.Value != element) {
                    return PlayerElement.None;
                }
            }
    
            return GetPlayerElementForId(firstElement.Value);
        }

        bool IsTimerRunning() => _hillCaptureTimer.GetRemainingTime() > 0;

        void Update() {
            if (IsServer) {
                _hillCaptureTimer?.Update();
                
                if (_isCapturing && IsTimerRunning()) {
                    UpdateCaptureColor();
                }
            }
            else {  
                if (_isCapturing) {
                    UpdateCaptureColor();
                }
            }
        }
        void UpdateCaptureColor() {
            if (_spriteRenderer == null) return;
            
            float elapsedTime = Time.time - _captureStartTime;
            float progress = Mathf.Clamp01(elapsedTime / _captureDuration);
            
            _spriteRenderer.color = Color.Lerp(_startColor, _targetColor, progress);
        }
        
        void OnHillElementChanged(PlayerElement previous, PlayerElement current) {
            UpdateHillColor(current);
            _isCapturing = false; 
        }

        void UpdateHillColor(PlayerElement element) {
            if (_spriteRenderer != null)
                _spriteRenderer.color = GetColorHillByElement(element);
        }

        Color GetColorHillByElement(PlayerElement playerElement) =>
            playerElement switch {
                PlayerElement.None => Color.gray,
                PlayerElement.Ice => Color.blue,
                PlayerElement.Fire => Color.red,
                _ => Color.black
            };
        public override void OnNetworkDespawn() {
            if (IsServer) {
                _hillCaptureTimer.OnTimerComplete -= OnCaptureComplete;
            }
            _hillElement.OnValueChanged -= OnHillElementChanged;
            base.OnNetworkDespawn();
        }
    }
}