using System;
using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Services;
using _PrismWars._Scripts.UI.CharacterSelection;
using _PrismWars._Scripts.UI.Services.Mono;
using _PrismWars._Scripts.Utils;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.UI {
    [RequireComponent(typeof(NetworkObject))]
    public class ClientUIManager : NetworkBehaviour, IService, IInitializable {
        [SerializeField] TextMeshProUGUI _timer;
        [SerializeField] CharacterSelectionManager _characterSelectionManager;
        [SerializeField] GameObject _waitingUI;
        [SerializeField] GameObject _timerUI;
        [SerializeField] GameUIViewService _gameUIViewService;
        [SerializeField] GameObject _respawnUI;
        [SerializeField] TextMeshProUGUI _respawnTimerText;
        
        GameOverUIService _gameOverUIService;
        
        Timer _respawnTimer;
        
        bool _isDead;
        
        public event Action OnCharacterSelectionConfirmed;
        
        NetworkTimer _networkTimer;

        public void Initialize() {
            _networkTimer = ServiceLocator.Singleton.Get<NetworkTimer>();
            _gameOverUIService = ServiceLocator.Singleton.Get<GameOverUIService>();
            
            ServiceLocator.Singleton.Register(_gameUIViewService);
            _gameUIViewService.Initialize();
        }

        public void SetRespawnTimer(Timer respawnTimer) {
            _respawnTimer = respawnTimer;
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void OnSelectCharacterClientRpc() {
            _characterSelectionManager.Initialize();
            _gameOverUIService.HideView();
            _waitingUI.gameObject.SetActive(false);
            _timerUI.gameObject.SetActive(true);
            _characterSelectionManager.View.ShowView();
        }
        
        [Rpc(SendTo.ClientsAndHost)]
        public void OnGameStartedClientRpc() {
            OnCharacterSelectionConfirmed?.Invoke();
            _gameUIViewService.ShowView();
        }
        [Rpc(SendTo.ClientsAndHost)]
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
            if(!_networkTimer) return;
            var time = (int)_networkTimer.GetRemainingTime();
            _timer.text = time.ToString();
            if(!_isDead) return;
            var respawnTime = (int)_respawnTimer!.GetRemainingTime();
            _respawnTimerText.text = $"Time to respawn: {respawnTime}";
        }
    }
}