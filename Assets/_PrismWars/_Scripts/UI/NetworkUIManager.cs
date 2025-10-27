using System;
using _PrismWars._Scripts.Utils;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.UI {
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkUIManager : NetworkBehaviour, IService, IInitializable {
        [SerializeField] TextMeshProUGUI _timer;
        [SerializeField] GameUIView _gameUIView;
        [SerializeField] CharacterSelectionManager _characterSelectionManager;
        [SerializeField] GameObject _waitingUI;
        [SerializeField] GameObject _timerUI;
        public event Action OnCharacterSelectionConfirmed;
        
        NetworkTimer _networkTimer;

        public void Initialize() {
            _networkTimer = ServiceLocator.Singleton.Get<NetworkTimer>();
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
                OnCharacterSelectionConfirmed?.Invoke();
                _gameUIView.ShowView();
            }
        }
        
        void Awake() {
            _timerUI.gameObject.SetActive(false);
            _waitingUI.gameObject.SetActive(true);
            _characterSelectionManager.View.HideView();
            _gameUIView.HideView();
            _timer.text = "0";
            
            _timerUI.gameObject.SetActive(false);
        }

        void Update() {
            var time = (int)_networkTimer.GetRemainingTime();
            _timer.text = time.ToString();
        }
    }
}