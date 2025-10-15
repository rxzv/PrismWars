using System;
using System.Collections.Generic;

namespace _PrismWars._Scripts.UI.Model {
    public class CharacterSelectionModel {
        public event Action<int> OnCharacterSelected;
        public event Action<int> OnCharacterHighlighted;
        public event Action<int> OnSelectionConfirmed;

        readonly List<PlayerConfig> _availableCharacters;
        readonly HashSet<int> _selectedCharacters;
        
        public int SelectedCharacterIndex { get; private set; } = -1;
        public int HighlightedCharacterIndex { get; private set; } = -1;

        public CharacterSelectionModel(List<PlayerConfig> characters) {
            _availableCharacters = characters;
            _selectedCharacters = new HashSet<int>();
        }

        public void SelectedCharacterUpdate(int unavailableIndex) {
            _selectedCharacters.Add(unavailableIndex);
        }

        public void SelectCharacter(int index) {
            if (index < 0 || index >= _availableCharacters.Count) return;
            if (_selectedCharacters.Contains(index)) return;

            var previousSelection = SelectedCharacterIndex;
            SelectedCharacterIndex = index;
            
            OnCharacterSelected?.Invoke(previousSelection);
        }

        public void HighlightCharacter(int index) {
            if (index < 0 || index >= _availableCharacters.Count) return;
            
            HighlightedCharacterIndex = index;
            OnCharacterHighlighted?.Invoke(index);
        }

        public void ConfirmSelection() {
            if (SelectedCharacterIndex == -1) return;
            
            OnSelectionConfirmed?.Invoke(SelectedCharacterIndex);
        }

        public void ReleaseCharacter(int index) {
            _selectedCharacters.Remove(index);
            if (SelectedCharacterIndex == index) {
                SelectedCharacterIndex = -1;
            }
        }

        public PlayerConfig GetCharacterConfig(int index) {
            return index >= 0 && index < _availableCharacters.Count ? _availableCharacters[index] : null;
        }

        public bool IsCharacterAvailable(int index) {
            return !_selectedCharacters.Contains(index);
        }

        public List<PlayerConfig> GetAvailableCharacters() => _availableCharacters;
    }
}