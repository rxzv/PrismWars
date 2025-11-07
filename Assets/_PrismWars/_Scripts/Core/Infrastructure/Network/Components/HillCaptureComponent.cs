using _PrismWars._Scripts.Game.Services;
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

        const int ADD_SCORE_COUNT = 1;
        const float DELAY_FOR_ADD_SCORE = 1;
        Timer _addScoreTimer;

        NetworkScoreService _scoreService;
        
        // TODO: Remove [SerializeField] to complete
        // TODO: Refactor
        // TODO: server time, dont local time: Time.time
        //TODO: add death event to change player count
        [SerializeField] NetworkVariable<Color> _startColor = new ();
        [SerializeField] NetworkVariable<Color> _targetColor = new();
        [SerializeField] NetworkVariable<float> _captureTime = new ();
        [SerializeField] NetworkVariable<bool> _isCapturing = new ();
        [SerializeField] NetworkVariable<float> _timeSaver = new ();
        [SerializeField] NetworkVariable<PlayerElement> _lastPlayerElement = new ();
        [SerializeField] NetworkVariable<bool> _isNewElement = new();
        [SerializeField] NetworkVariable<float> _progress = new();
        [SerializeField] NetworkVariable<Color> _saverColor = new();
        [SerializeField] NetworkVariable<float> _elapsedTime = new();

        public override void OnNetworkSpawn() {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            UpdateHillColor(_hillElement.Value);
            
            if (IsServer) {
                _lastPlayerElement.Value = GetDominantElement();
                _startColor.Value = GetColorHillByElement(_hillElement.Value);
                _timeSaver.Value = _captureDuration.Value; 
                _saverColor.Value = GetColorHillByElement(_hillElement.Value);
                _elapsedTime.Value = Time.time;
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
                _elapsedTime.Value = Time.time;
                _timeSaver.Value = _captureDuration.Value;  
                _startColor.Value = GetColorHillByElement(_hillElement.Value);
                _lastPlayerElement.Value = PlayerElement.None;
                _isNewElement.Value = false;
                return;
            }

            PlayerElement currentElement = GetDominantElement();
            // если игроки разных элементов
            if (currentElement == PlayerElement.None) {
                if (IsTimerRunning()) {
                    _startColor.Value = _saverColor.Value;
                    _timeSaver.Value = _hillCaptureTimer.GetRemainingTime();
                    _hillCaptureTimer.ResetTimer();
                    _isCapturing.Value = false;
                    _captureTime.Value = Time.time - (_captureDuration.Value - _timeSaver.Value);
                }
            } // Начинаем или продолжаем захват
            else if (_hillElement.Value != currentElement) {
                _targetColor.Value = GetColorHillByElement(currentElement);
                if (!IsTimerRunning()) {
                    // Новый элемент - начинаем заново
                    if (_lastPlayerElement.Value != currentElement) {
                        _startColor.Value = GetColorHillByElement(_hillElement.Value);
                        _lastPlayerElement.Value = currentElement;
                        _captureTime.Value = Time.time;
                        _hillCaptureTimer.StartTimer(_captureDuration.Value);
                        _isNewElement.Value = true;
                        _elapsedTime.Value = Time.time;
                    }
                    else { // Тот же элемент - продолжаем с сохраненного времени
                        _captureTime.Value = Time.time - (_captureDuration.Value - _timeSaver.Value);
                        _hillCaptureTimer.StartTimer(_timeSaver.Value);
                        _isNewElement.Value = false;
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
                _timeSaver.Value = _captureDuration.Value;
                _progress.Value = 0;
                _elapsedTime.Value = Time.time;
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
            if (_hillElement.Value != PlayerElement.None) 
                _scoreService.AddScore(_hillElement.Value, ADD_SCORE_COUNT);
        }
        
        void UpdateCaptureColor() {
            if (_spriteRenderer == null) return;

            if (IsServer) {
                if (_isNewElement.Value) 
                    _elapsedTime.Value = Time.time - _captureTime.Value;
                
                _progress.Value = Mathf.Clamp01(_elapsedTime.Value / _captureDuration.Value);
                
                _saverColor.Value = _spriteRenderer.color =
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