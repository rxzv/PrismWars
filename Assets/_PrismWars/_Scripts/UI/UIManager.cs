using System;
using _PrismWars._Scripts.Game.GameManager;
using UnityEngine;

namespace _PrismWars._Scripts.UI {
    public class UIManager : MonoBehaviour, IService, IInitializable {

        [SerializeField] GameUIManager _gameUIManager;
        [SerializeField] CharacterClientSelectionManager _characterClientSelectionManager;
        
        public event Action OnCharacterSelectionConfirmed;
        
        ServerGameManager _serverGameManager;

        public void Initialize() {
            _serverGameManager = ServiceLocator.Singleton.Get<ServerGameManager>();
            _serverGameManager.OnGameStarted += OnGameStarted;
            _serverGameManager.OnSelectCharacter += OnSelectCharacter;
        }
        void Awake() {
            _characterClientSelectionManager.gameObject.SetActive(false);
            _gameUIManager.gameObject.SetActive(false);
        }

        void OnSelectCharacter() {
            _characterClientSelectionManager.gameObject.SetActive(true);
            _characterClientSelectionManager.Initialize();
        }
        
        void OnGameStarted() {
            OnCharacterSelectionConfirmed?.Invoke();
            _gameUIManager.gameObject.SetActive(true);
        }

        void OnDestroy() {
            _serverGameManager.OnGameStarted -= OnGameStarted;
        }

    }
}