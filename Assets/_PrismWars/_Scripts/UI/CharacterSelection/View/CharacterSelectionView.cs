using System;
using System.Collections.Generic;
using _PrismWars._Scripts.UI.Model;
using UnityEngine;
using UnityEngine.UI;

namespace _PrismWars._Scripts.UI.View {
    public class CharacterSelectionView : MonoBehaviour {
        [SerializeField] List<CharacterButton> _characterButtons;
        [SerializeField] Color _fireColor = Color.red;
        [SerializeField] Color _iceColor = Color.blue;
        [SerializeField] Button _confirmButton;

        public event Action<int> OnCharacterButtonClicked;
        public event Action OnConfrimButtonClicked;
        public event Action OnGameReady;
        
        CharacterServerSelectionManager _characterServerSelectionManager;

        void Start() {
            for (var i = 0; i < _characterButtons.Count; i++) {
                var index = i;
                _characterButtons[i].button.onClick.AddListener(() => OnCharacterButtonClicked?.Invoke(index));
            }
            _confirmButton.onClick.AddListener(() => OnConfrimButtonClicked?.Invoke());
        }

        public void ConfirmButtonClicked() {
            _confirmButton.onClick.RemoveAllListeners();
            _confirmButton.gameObject.GetComponent<Image>().color = Color.red;
            _confirmButton.GetComponentInChildren<Text>().text = "Ready";
            OnGameReady?.Invoke();
        }

        public void InitializeCharacters(List<PlayerConfig> characterConfigs) {
            for (int i = 0; i < _characterButtons.Count && i < characterConfigs.Count; i++) {
                PlayerConfig config = characterConfigs[i];
                CharacterButton characterButton = _characterButtons[i];

                characterButton.characterImage.sprite = config.sprite;
                characterButton.backgroundImage.color = GetColorByPlayerElement(config.playerElement);
                characterButton.selectionFrame.SetActive(false);
            }

            _characterServerSelectionManager = ServiceLocator.Singleton.Get<CharacterServerSelectionManager>();
        }

        public void SetCharacterSelected(int characterIndex, bool selected) {
            if (characterIndex >= 0 && characterIndex < _characterButtons.Count) {
                _characterButtons[characterIndex].selectionFrame.SetActive(selected);
            }
        }
        
        public void HideView() => gameObject.SetActive(false);

        public void HighlightUnavaliableCharacter(int characterIndex) {
            _characterButtons[characterIndex].selectedImageFrame.gameObject.SetActive(true);
        }

        public void ShowCharacterUnavailableMessage(int characterIndex) {
            Debug.Log($"Персонаж {characterIndex} уже выбран другим игроком!");
            // Можно добавить визуальную обратную связь
        }

        public void ShowNoCharacterSelectedMessage() {
            Debug.Log("Пожалуйста, выберите персонажа!");
        }

        public void ShowSelectionConfirmed(int id) {
            Debug.Log("Выбор персонажа подтвержден!");
            _characterServerSelectionManager.SelectCharacterServerRpc(id);
        }

        Color GetColorByPlayerElement(PlayerElement playerElement) {
            return playerElement switch {
                PlayerElement.Fire => _fireColor,
                PlayerElement.Ice => _iceColor,
                _ => Color.white
            };
        }
    }
}