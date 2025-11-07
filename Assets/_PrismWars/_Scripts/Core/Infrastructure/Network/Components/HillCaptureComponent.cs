using _PrismWars._Scripts.Game.Services;
using _PrismWars._Scripts.Player;
using _PrismWars._Scripts.UI.Model;
using _PrismWars._Scripts.Utils;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Infrastructure.Network.Components {
    public class HillCaptureComponent : NetworkBehaviour {
        NetworkVariable<PlayerElement> _hillElement = new ();
        NetworkList<int> _hillPlayers = new();
        
        Timer _hillCaptureTimer;
        SpriteRenderer _spriteRenderer;

        const int ADD_SCORE_COUNT = 1;
        const float DELAY_FOR_ADD_SCORE = 1;
        const float CAPTURE_DURATION = 5f;
        Timer _addScoreTimer;
        
        NetworkScoreService _scoreService;
        
        // TODO: Refactor
        // TODO: server time, dont local time: Time.time
        [SerializeField] NetworkVariable<Color> _startColor = new ();
        [SerializeField] NetworkVariable<Color> _targetColor = new();
        [SerializeField] NetworkVariable<float> _progress = new();
        [SerializeField] NetworkVariable<bool> _isCapturing = new ();
        
        float _captureTime;
        float _timeSaver;
        PlayerElement _lastPlayerElement;
        bool _isNewElement;
        Color _saverColor;
        float _elapsedTime;

        public override void OnNetworkSpawn() {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            UpdateHillColor(_hillElement.Value);
            
            if (IsServer) {
                _lastPlayerElement = GetDominantElement();
                _startColor.Value = GetColorHillByElement(_hillElement.Value);
                _timeSaver = CAPTURE_DURATION; 
                _saverColor = GetColorHillByElement(_hillElement.Value);
                _elapsedTime = Time.time;
                _hillCaptureTimer = new Timer();
                _hillCaptureTimer.OnTimerComplete += OnCaptureComplete;
                _hillPlayers.OnListChanged += HillPlayersCountChanged;
                _addScoreTimer = new Timer();
                _addScoreTimer.OnTimerComplete += AddScoreTimerComplete;
                _scoreService = ServiceLocator.Singleton.Get<NetworkScoreService>();
            }
            
            _hillElement.OnValueChanged += OnHillElementChanged;
            base.OnNetworkSpawn();
        }

        void AddScoreTimerComplete() {
            if(!IsServer) return;
            AddScoreForDominantElement();
            _addScoreTimer.StartTimer(DELAY_FOR_ADD_SCORE);
        }

        void HillPlayersCountChanged(NetworkListEvent<int> changeEvent) {
            if (!IsServer) return;
            // если игроков в зоне 0
            if (_hillPlayers.Count == 0) {
                if(IsTimerRunning())
                    _hillCaptureTimer.StopTimer();
                _isCapturing.Value = false;
                _progress.Value = 0f;
                _elapsedTime = Time.time;
                _timeSaver = CAPTURE_DURATION;  
                _startColor.Value = GetColorHillByElement(_hillElement.Value);
                _lastPlayerElement = PlayerElement.None;
                _isNewElement = false;
                return;
            }

            PlayerElement currentElement = GetDominantElement();
            // если игроки разных элементов
            if (currentElement == PlayerElement.None) {
                if (IsTimerRunning()) {
                    _startColor.Value = _saverColor;
                    _timeSaver = _hillCaptureTimer.GetRemainingTime();
                    _hillCaptureTimer.ResetTimer();
                    _isCapturing.Value = false;
                    _captureTime = Time.time - (CAPTURE_DURATION - _timeSaver);
                }
            } // Начинаем или продолжаем захват
            else if (_hillElement.Value != currentElement) {
                _targetColor.Value = GetColorHillByElement(currentElement);
                if (!IsTimerRunning()) {
                    // Новый элемент - начинаем заново
                    if (_lastPlayerElement != currentElement) {
                        _startColor.Value = GetColorHillByElement(_hillElement.Value);
                        _lastPlayerElement = currentElement;
                        _captureTime = Time.time;
                        _hillCaptureTimer.StartTimer(CAPTURE_DURATION);
                        _isNewElement = true;
                        _elapsedTime = Time.time;
                    }
                    else { 
                        // Тот же элемент - продолжаем с сохраненного времени
                        _captureTime = Time.time - (CAPTURE_DURATION - _timeSaver);
                        _hillCaptureTimer.StartTimer(_timeSaver);
                        _isNewElement = false;
                    }
                    _isCapturing.Value = true;
                }
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
                _timeSaver = CAPTURE_DURATION;
                _progress.Value = 0;
                _elapsedTime = Time.time;
                _isCapturing.Value = false;
                _addScoreTimer.StopTimer();
                _addScoreTimer.StartTimer(DELAY_FOR_ADD_SCORE);
            }
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

        bool IsTimerRunning() => _hillCaptureTimer != null && _hillCaptureTimer.GetRemainingTime() > 0; 

        void Update() {
            _hillCaptureTimer?.Update();
            _addScoreTimer?.Update();
            
            if (_isCapturing.Value) 
                UpdateCaptureColor();
            else if(_hillPlayers.Count == 0) 
                _spriteRenderer.color = GetColorHillByElement(_hillElement.Value);
        }

        void AddScoreForDominantElement() {
            if(!IsServer) return;
            if (_hillElement.Value != PlayerElement.None) 
                _scoreService.AddScore(_hillElement.Value, ADD_SCORE_COUNT);
        }
        
        void UpdateCaptureColor() {
            if (_spriteRenderer == null) return;

            if (IsServer) {
                if (_isNewElement) 
                    _elapsedTime = Time.time - _captureTime;
                
                _progress.Value = Mathf.Clamp01(_elapsedTime / CAPTURE_DURATION);
                
                _saverColor = _spriteRenderer.color =
                    Color.Lerp(_startColor.Value, _targetColor.Value, _progress.Value);
            }
            else if(IsClient)
                _spriteRenderer.color =
                    Color.Lerp(_startColor.Value, _targetColor.Value, _progress.Value);
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
                _hillPlayers.OnListChanged -= HillPlayersCountChanged;
                _addScoreTimer.OnTimerComplete -= AddScoreTimerComplete;
            }
            _hillElement.OnValueChanged -= OnHillElementChanged;
            base.OnNetworkDespawn();
        }
    }
}