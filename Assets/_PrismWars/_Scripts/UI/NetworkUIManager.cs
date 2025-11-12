using System;
using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Services;
using _PrismWars._Scripts.Utils;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.UI {
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkUIManager : NetworkBehaviour, IService, IInitializable {
        [SerializeField] TextMeshProUGUI _timer;
        [SerializeField] CharacterSelectionManager _characterSelectionManager;
        [SerializeField] GameObject _waitingUI;
        [SerializeField] GameObject _timerUI;
        [SerializeField] GameUIViewService _gameUIViewService;
        [SerializeField] GameObject _respawnUI;
        [SerializeField] TextMeshProUGUI _respawnTimerText;
        
        GameOverUIService _gameOverUIService;
        
        RespawnTimer _respawnTimer;
        
        bool _isDead = false;
        
        public event Action OnCharacterSelectionConfirmed;
        
        NetworkGameTimer _networkGameTimer;

        public void Initialize() {
            _respawnTimer = ServiceLocator.Singleton.Get<RespawnTimer>();
            _networkGameTimer = ServiceLocator.Singleton.Get<NetworkGameTimer>();
            _gameOverUIService = ServiceLocator.Singleton.Get<GameOverUIService>();
            
            ServiceLocator.Singleton.Register(_gameUIViewService);
            _gameUIViewService.Initialize();
        }

        [ClientRpc]
        public void OnSelectCharacterClientRpc() {
            _characterSelectionManager.Initialize();
            _gameOverUIService.HideView();
            _waitingUI.gameObject.SetActive(false);
            _timerUI.gameObject.SetActive(true);
            _characterSelectionManager.View.ShowView();
        }
        
        [ClientRpc]
        public void OnGameStartedClientRpc() {
            OnCharacterSelectionConfirmed?.Invoke();
            _gameUIViewService.ShowView();
        }
        [ClientRpc]
        public void OnGameOverClientRpc() {
            _gameOverUIService.ShowView();
            _gameOverUIService.GameOver();
        }
        public void OnPlayerDead() {
            if (IsClient) {
                _respawnUI.gameObject.SetActive(true);
                _isDead = true;
            }
        }
        public void OnPlayerRespawn() {
            if (IsClient) {
                _respawnUI.gameObject.SetActive(false);
                _isDead = false;
            }
        }
        void Awake() {
            _respawnUI.gameObject.SetActive(false);
            _timerUI.gameObject.SetActive(false);
            _waitingUI.gameObject.SetActive(true);
            _characterSelectionManager.View.HideView();
            _gameUIViewService.HideView();
            _timer.text = "0";
            
            _timerUI.gameObject.SetActive(false);
        }

        void Update() {
            if(!_networkGameTimer) return;
            var time = (int)_networkGameTimer.GetRemainingTime();
            _timer.text = time.ToString();
            if (_isDead) {
                var respawnTime = (int)_respawnTimer.GetRemainingTime();
                _respawnTimerText.text = $"Time to respawn: {respawnTime}";
            }
        }
    }
}