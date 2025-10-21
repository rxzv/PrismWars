using System;
using System.Collections.Generic;
using System.Linq;

namespace _PrismWars._Scripts.UI.Model {
    public class CharacterSelectionModel {
        public event Action<int> OnCharacterSelected;
        public event Action<int> OnCharacterUnavaliableHighlighted;
        public event Action<int> OnSelectionConfirmed;

        readonly List<PlayerConfig> _availableCharacters;
        readonly HashSet<int> _selectedCharactersId;
        
        public int SelectedCharacterButtonIndex { get; private set; } = -1;
        public int HighlightedCharacterIndex { get; private set; } = -1;

        public CharacterSelectionModel(List<PlayerConfig> characters) {
            _availableCharacters = characters;
            _selectedCharactersId = new HashSet<int>();
        }

        public void SelectedCharacterUpdate(int id) {
            _selectedCharactersId.Add(id);
            HighlightUnavailableCharacter(id);
        }

        public void SelectCharacter(int index) {
            if (index < 0 || index >= _availableCharacters.Count) return;
            if (_selectedCharactersId.Contains(_availableCharacters[index].configId)) return;

            var previousSelection = SelectedCharacterButtonIndex;
            SelectedCharacterButtonIndex = index;
            
            OnCharacterSelected?.Invoke(previousSelection);
        }

        public void HighlightUnavailableCharacter(int id) {
            if (id < 0 || id >= _availableCharacters.Count) return;
            
            PlayerConfig config = _availableCharacters.FirstOrDefault(p => p.configId == id);
            if (config == null) return;
            
            HighlightedCharacterIndex = _availableCharacters.IndexOf(config);
            OnCharacterUnavaliableHighlighted?.Invoke(HighlightedCharacterIndex);
        }

        public void ConfirmSelection() {
            if (SelectedCharacterButtonIndex == -1) return;
            
            OnSelectionConfirmed?.Invoke(_availableCharacters[SelectedCharacterButtonIndex].configId);
        }

        public void ReleaseCharacter(int index) {
            _selectedCharactersId.Remove(index);
            if (SelectedCharacterButtonIndex == index) {
                SelectedCharacterButtonIndex = -1;
            }
        }

        public PlayerConfig GetCharacterConfig(int index) {
            return index >= 0 && index < _availableCharacters.Count ? _availableCharacters[index] : null;
        }
        public PlayerConfig GetRandomCharacterConfig() {
            var rand = new Random();
            while (true) {
                int randId = rand.Next(0, _availableCharacters.Count);
                if(IsCharacterAvailable(randId))
                    return GetCharacterConfig(randId);
            }
        }

        public bool IsCharacterAvailable(int index) {
            return !_selectedCharactersId.Contains(_availableCharacters[index].configId);
        }

        public List<PlayerConfig> GetAvailableCharacters() => _availableCharacters;
    }
}