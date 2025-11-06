using _PrismWars._Scripts.Player;
using _PrismWars._Scripts.UI.Model;
using _PrismWars._Scripts.Utils;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Infrastructure.Network.Components {
    public class HillCaptureComponent : NetworkBehaviour {
        [SerializeField] NetworkVariable<float> _captureDuration = new(5f);
        
        [SerializeField] NetworkVariable<PlayerElement> _hillElement = new ();
        NetworkList<int> _hillPlayers = new();
        
        Timer _hillCaptureTimer;
        SpriteRenderer _spriteRenderer;
        [SerializeField] NetworkVariable<Color> _targetColor = new();
        [SerializeField] NetworkVariable<float> _captureStartTime = new ();
        [SerializeField] NetworkVariable<bool> _isCapturing = new ();
        [SerializeField] NetworkVariable<float> _timeSaver = new ();
        [SerializeField] NetworkVariable<PlayerElement> _lastPlayerElement = new ();
        [SerializeField] NetworkVariable<Color> _lastColor = new ();
        [SerializeField] NetworkVariable<bool> _isNewElement = new();

        public override void OnNetworkSpawn() {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            UpdateHillColor(_hillElement.Value);
            
            if (IsServer) {
                _lastPlayerElement.Value = GetDominantElement();
                _lastColor.Value = GetColorHillByElement(_hillElement.Value);
                _timeSaver.Value = _captureDuration.Value; 
                _hillCaptureTimer = new Timer();
                _hillCaptureTimer.OnTimerComplete += OnCaptureComplete;
                _hillPlayers.OnListChanged += HillPlayersCountChanged;
            }
            
            _hillElement.OnValueChanged += OnHillElementChanged;
            base.OnNetworkSpawn();
        }

        void HillPlayersCountChanged(NetworkListEvent<int> changeEvent) {
            if (!IsServer) return;
            if (_hillPlayers.Count == 0) {
                if(IsTimerRunning())
                    _hillCaptureTimer.StopTimer();
                _isCapturing.Value = false;
                _spriteRenderer.color = GetColorHillByElement(_hillElement.Value);
                return;
            }

            PlayerElement currentElement = GetDominantElement();

            if (currentElement == PlayerElement.None) {
                if (IsTimerRunning()) {
                    _timeSaver.Value = _hillCaptureTimer.GetRemainingTime();
                    _hillCaptureTimer.ResetTimer();
                    _isCapturing.Value = false;
                }
            }
            else if (!IsTimerRunning() && _hillElement.Value != currentElement) {
                _targetColor.Value = GetColorHillByElement(currentElement);
                _captureStartTime.Value = Time.time;
                
                if (_lastPlayerElement.Value != currentElement) {
                    _lastPlayerElement.Value = currentElement;
                    _hillCaptureTimer.StartTimer(_captureDuration.Value);
                    _isNewElement.Value = true;
                }
                else {
                    _lastPlayerElement.Value = currentElement;
                    _hillCaptureTimer.StartTimer(_timeSaver.Value);
                    _isNewElement.Value = false;
                }
                _isCapturing.Value = true;
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
            if (dominantElement != PlayerElement.None) {
                _hillElement.Value = dominantElement;
                _timeSaver.Value = _captureDuration.Value; 
            }
        }
        PlayerElement GetDominantElement() {
            if (_hillPlayers.Count == 0) 
                return _hillElement.Value;

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
                
                if (_isCapturing.Value && IsTimerRunning()) {
                    UpdateCaptureColor();
                } else if(_hillPlayers.Count < 2) {
                    _spriteRenderer.color = GetColorHillByElement(_hillElement.Value);
                }
            }
            else {  
                if (_isCapturing.Value) {
                    UpdateCaptureColor();
                } else if(_hillPlayers.Count < 2) {
                    _spriteRenderer.color = GetColorHillByElement(_hillElement.Value);
                }
            }
        }
        void UpdateCaptureColor() {
            if (_spriteRenderer == null) return;
            
            float elapsedTime = Time.time - _captureStartTime.Value;
            float progress = Mathf.Clamp01(elapsedTime / _captureDuration.Value);
            if (IsServer) {
                if (_isNewElement.Value) 
                    _lastColor.Value = _spriteRenderer.color =
                        Color.Lerp(_lastColor.Value, _targetColor.Value, progress);
                else 
                    _lastColor.Value = _spriteRenderer.color =
                        Color.Lerp(_lastColor.Value, _targetColor.Value, progress);
            }
            else {
                if (_isNewElement.Value) 
                    _spriteRenderer.color =
                        Color.Lerp(_lastColor.Value, _targetColor.Value, progress);
                else 
                    _spriteRenderer.color =
                        Color.Lerp(_lastColor.Value, _targetColor.Value, progress); 
            }
        }
        
        void OnHillElementChanged(PlayerElement previous, PlayerElement current) {
            UpdateHillColor(current);
            if(!IsServer) return;
            _isCapturing.Value = false; 
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