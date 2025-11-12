using _PrismWars._Scripts.Core.Patterns.Command;
using _PrismWars._Scripts.Game.Player.Model;
using _PrismWars._Scripts.UI.CharacterSelection.Model;
using _PrismWars._Scripts.UI.CharacterSelection.View;

namespace _PrismWars._Scripts.UI.CharacterSelection.Controller {
    public class CharacterSelectionController {
        readonly CharacterSelectionModel _model;
        readonly CharacterSelectionView _view;
        readonly CommandInvoker _commandInvoker;
        readonly ClientUIManager _clientUIManager;

        public CharacterSelectionController(CharacterSelectionModel model, CharacterSelectionView view) {
            _clientUIManager = ServiceLocator.Singleton.Get<ClientUIManager>();
            _model = model;
            _view = view;
            _commandInvoker = new CommandInvoker();

            _clientUIManager.OnCharacterSelectionConfirmed += OnConfirmCharacterSelectedExecute;
            
            _model.OnCharacterSelected += OnCharacterSelected;
            _model.OnCharacterUnavailableHighlighted += OnCharacterUnavailableHighlighted;
            _model.OnSelectionConfirmed += OnSelectionConfirmed;

            _view.OnCharacterButtonClicked += OnCharacterButtonClicked;
            _view.OnConfirmButtonClicked += OnConfirmCharacterSelection;
        }

        void OnCharacterButtonClicked(int index) {
            if (!_model.IsCharacterAvailable(index)) {
                _view.ShowCharacterUnavailableMessage(index);
                return;
            }

            var command = new SelectCharacterCommand(this, index);
            _commandInvoker.ExecuteCommand(command);
        }

        public void SelectCharacter(int characterIndex) => _model.SelectCharacter(characterIndex);

        public int GetSelectedCharacterIndex() => _model.SelectedCharacterButtonIndex;
        

        void OnCharacterSelected(int previousSelection) {
            if (previousSelection != -1)
                _view.SetCharacterSelected(previousSelection, false);

            if (_model.SelectedCharacterButtonIndex != -1)
                _view.SetCharacterSelected(_model.SelectedCharacterButtonIndex, true);
        }

        void OnCharacterUnavailableHighlighted(int index) => 
            _view.HighlightUnavailableCharacter(index);
        

        void OnSelectionConfirmed(int id) => _view.ShowSelectionConfirmed(id);

        void OnConfirmCharacterSelection() {
            if (_model.SelectedCharacterButtonIndex == -1) {
                _view.ShowNoCharacterSelectedMessage();
                return;
            }
            _model.ConfirmSelection();

            _view.ConfirmButtonClicked();
        }

        void OnConfirmCharacterSelectedExecute() {
            PlayerConfig playerConfig;
            
            if (_model.SelectedCharacterButtonIndex == -1) 
                playerConfig = _model.GetRandomCharacterConfig();
            else 
                playerConfig = _model.GetCharacterConfig(_model.SelectedCharacterButtonIndex);;
            
            var command = new ConfirmCharacterSelected(playerConfig);
            _commandInvoker.ExecuteCommand(command);
            _view.HideView();
        }

        public void Cleanup() {
            _clientUIManager.OnCharacterSelectionConfirmed += OnConfirmCharacterSelectedExecute;
            
            _model.OnCharacterSelected -= OnCharacterSelected;
            _model.OnCharacterUnavailableHighlighted -= OnCharacterUnavailableHighlighted;
            _model.OnSelectionConfirmed -= OnSelectionConfirmed;

            _view.OnCharacterButtonClicked -= OnCharacterButtonClicked;
        }
    }
}