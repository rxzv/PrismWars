using _PrismWars._Scripts.UI.Command;
using _PrismWars._Scripts.UI.Model;
using _PrismWars._Scripts.UI.View;

namespace _PrismWars._Scripts.UI.Controller {
    public class CharacterSelectionController {
        readonly CharacterSelectionModel _model;
        readonly CharacterSelectionView _view;
        readonly CommandInvoker _commandInvoker;

        public CharacterSelectionController(CharacterSelectionModel model, CharacterSelectionView view) {
            _model = model;
            _view = view;
            _commandInvoker = new CommandInvoker();

            _model.OnCharacterSelected += OnCharacterSelected;
            _model.OnCharacterUnavaliableHighlighted += OnCharacterUnavaliableHighlighted;
            _model.OnSelectionConfirmed += OnSelectionConfirmed;

            _view.OnCharacterButtonClicked += OnCharacterButtonClicked;
            _view.OnConfrimButtonClicked += OnConfirmCharacterSelection;
        }

        void OnCharacterButtonClicked(int characterIndex) {
            if (!_model.IsCharacterAvailable(characterIndex)) {
                _view.ShowCharacterUnavailableMessage(characterIndex);
                return;
            }

            var command = new SelectCharacterCommand(this, characterIndex);
            _commandInvoker.ExecuteCommand(command);
        }

        public void SelectCharacter(int characterIndex) => _model.SelectCharacter(characterIndex);

        public int GetSelectedCharacterIndex() => _model.SelectedCharacterIndex;
        

        void OnCharacterSelected(int previousSelection) {
            if (previousSelection != -1)
                _view.SetCharacterSelected(previousSelection, false);

            if (_model.SelectedCharacterIndex != -1)
                _view.SetCharacterSelected(_model.SelectedCharacterIndex, true);
        }

        void OnCharacterUnavaliableHighlighted(int characterIndex) => _view.HighlightUnavaliableCharacter(characterIndex);
        

        void OnSelectionConfirmed(int index) => _view.ShowSelectionConfirmed(index);

        void OnConfirmCharacterSelection() {
            if (_model.SelectedCharacterIndex == -1) {
                _view.ShowNoCharacterSelectedMessage();
                return;
            }

            _model.ConfirmSelection();
            var playerConfig = _model.GetCharacterConfig(_model.SelectedCharacterIndex);
            var command = new ConfirmCharacterSelected(playerConfig);
            _commandInvoker.ExecuteCommand(command);
            _view.HideView();
        }

        public void Cleanup() {
            _model.OnCharacterSelected -= OnCharacterSelected;
            _model.OnCharacterUnavaliableHighlighted -= OnCharacterUnavaliableHighlighted;
            _model.OnSelectionConfirmed -= OnSelectionConfirmed;

            _view.OnCharacterButtonClicked -= OnCharacterButtonClicked;
        }
    }
}