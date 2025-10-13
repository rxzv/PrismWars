using System;
using System.Collections.Generic;
using _PrismWars._Scripts.UI.Model;
using UnityEngine;
using UnityEngine.UI;

namespace _PrismWars._Scripts.UI.View {
    public class CharacterSelectionView : MonoBehaviour {
        [SerializeField] List<CharacterButton> _characterButtons;
        [SerializeField] Button _startHostButton;
        [SerializeField] Button _startClientButton;
        [SerializeField] Color _fireColor = Color.red;
        [SerializeField] Color _iceColor = Color.blue;

        public event Action<int> OnCharacterButtonClicked;
        public event Action OnStartHostClicked;
        public event Action OnStartClientClicked;

        void Start() {
            for (var i = 0; i < _characterButtons.Count; i++) {
                var index = i;
                _characterButtons[i].button.onClick.AddListener(() => OnCharacterButtonClicked?.Invoke(index));
            }

            _startHostButton.onClick.AddListener(() => OnStartHostClicked?.Invoke());
            _startClientButton.onClick.AddListener(() => OnStartClientClicked?.Invoke());
        }

        public void InitializeCharacters(List<PlayerConfig> characterConfigs) {
            for (int i = 0; i < _characterButtons.Count && i < characterConfigs.Count; i++) {
                PlayerConfig config = characterConfigs[i];
                CharacterButton characterButton = _characterButtons[i];

                characterButton.characterImage.sprite = config.sprite;
                characterButton.backgroundImage.color = GetColorByPlayerType(config.playerType);
                characterButton.selectionFrame.SetActive(false);
            }
        }

        public void SetCharacterSelected(int characterIndex, bool selected) {
            if (characterIndex >= 0 && characterIndex < _characterButtons.Count) {
                _characterButtons[characterIndex].selectionFrame.SetActive(selected);
            }
        }
        
        public void HideView() => gameObject.SetActive(false);

        public void HighlightCharacter(int characterIndex) {
            // Реализация подсветки при наведении (опционально)
        }

        public void ShowCharacterUnavailableMessage(int characterIndex) {
            Debug.Log($"Персонаж {characterIndex} уже выбран другим игроком!");
            // Можно добавить визуальную обратную связь
        }

        public void ShowNoCharacterSelectedMessage() {
            Debug.Log("Пожалуйста, выберите персонажа!");
        }

        public void ShowSelectionConfirmed() {
            Debug.Log("Выбор персонажа подтвержден!");
        }

        Color GetColorByPlayerType(PlayerType playerType) {
            return playerType switch {
                PlayerType.Fire => _fireColor,
                PlayerType.Ice => _iceColor,
                _ => Color.white
            };
        }
    }
}