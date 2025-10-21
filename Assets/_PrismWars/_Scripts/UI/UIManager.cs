using System;
using _PrismWars._Scripts.Game.GameManager;
using _PrismWars._Scripts.Utils;
using TMPro;
using UnityEngine;

namespace _PrismWars._Scripts.UI {
    public class UIManager : MonoBehaviour, IService, IInitializable {

        [SerializeField] TextMeshProUGUI _timer;
        [SerializeField] GameUIView _gameUIView;
        [SerializeField] CharacterClientSelectionManager _characterClientSelectionManager;
        public event Action OnCharacterSelectionConfirmed;
        
        ServerGameManager _serverGameManager;
        NetworkTimer _networkTimer;

        public void Initialize() {
            _serverGameManager = ServiceLocator.Singleton.Get<ServerGameManager>();
            _networkTimer = ServiceLocator.Singleton.Get<NetworkTimer>();
            _serverGameManager.OnSelectCharacter += OnSelectCharacter;
            _serverGameManager.OnGameStarted += OnGameStarted;
        }
        
        void Awake() {
            _characterClientSelectionManager.View.HideView();
            _gameUIView.HideView();
            _timer.text = "0";
        }

        void Update() {
            var time = (int)_networkTimer.GetRemainingTime();
            _timer.text = time.ToString();
        }

        void OnSelectCharacter() {
            _characterClientSelectionManager.Initialize();
            _characterClientSelectionManager.View.ShowView();
        }
        
        void OnGameStarted() {
            OnCharacterSelectionConfirmed?.Invoke();
            _gameUIView.ShowView();
        }

        void OnDestroy() {
            _serverGameManager.OnGameStarted -= OnGameStarted;
        }

    }
}