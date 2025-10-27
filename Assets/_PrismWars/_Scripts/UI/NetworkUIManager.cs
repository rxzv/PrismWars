using System;
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
        
        RespawnTimer _respawnTimer;
        
        bool _isDead = false;
        
        public event Action OnCharacterSelectionConfirmed;
        
        NetworkGameTimer _networkGameTimer;

        public void Initialize() {
            _respawnTimer = ServiceLocator.Singleton.Get<RespawnTimer>();
            _networkGameTimer = ServiceLocator.Singleton.Get<NetworkGameTimer>();
        }

        [ClientRpc]
        public void OnSelectCharacterClientRpc() {
            if (IsClient) {
                _characterSelectionManager.Initialize();
                _waitingUI.gameObject.SetActive(false);
                _timerUI.gameObject.SetActive(true);
                _characterSelectionManager.View.ShowView();
            }
        }
        [ClientRpc]
        public void OnGameStartedClientRpc() {
            if (IsClient) {
                _gameUIViewService.ShowView();
                ServiceLocator.Singleton.Register(_gameUIViewService);
                OnCharacterSelectionConfirmed?.Invoke();
            }
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
            var time = (int)_networkGameTimer.GetRemainingTime();
            _timer.text = time.ToString();
            if (_isDead) {
                var respawnTime = (int)_respawnTimer.GetRemainingTime();
                _respawnTimerText.text = $"Time to respawn: {respawnTime}";
            }
        }
    }
}