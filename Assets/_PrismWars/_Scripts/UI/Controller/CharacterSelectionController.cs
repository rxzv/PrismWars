using _PrismWars._Scripts.UI.Command;
using _PrismWars._Scripts.UI.Model;
using _PrismWars._Scripts.UI.View;
using Unity.Netcode;

namespace _PrismWars._Scripts.UI.Controller {
    public class CharacterSelectionController {
        readonly CharacterSelectionModel _model;
        readonly CharacterSelectionView _view;
        readonly CommandInvoker _commandInvoker;
        readonly PlayerSpawnService _playerSpawnService;

        public CharacterSelectionController(CharacterSelectionModel model, CharacterSelectionView view, PlayerSpawnService playerSpawnService) {
            _model = model;
            _view = view;
            _playerSpawnService = playerSpawnService;
            _commandInvoker = new CommandInvoker();

            _model.OnCharacterSelected += OnCharacterSelected;
            _model.OnCharacterHighlighted += OnCharacterHighlighted;
            _model.OnSelectionConfirmed += OnSelectionConfirmed;

            _view.OnCharacterButtonClicked += OnCharacterButtonClicked;
            _view.OnStartHostClicked += OnStartHostClicked;
            _view.OnStartClientClicked += OnStartClientClicked;
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

        void OnCharacterHighlighted(int characterIndex) => _view.HighlightCharacter(characterIndex);
        

        void OnSelectionConfirmed() => _view.ShowSelectionConfirmed();
        

        void OnStartHostClicked() {
            if (_model.SelectedCharacterIndex == -1) {
                _view.ShowNoCharacterSelectedMessage();
                return;
            }

            var playerConfig = _model.GetCharacterConfig(_model.SelectedCharacterIndex);
            var command = new StartHostCommand(NetworkManager.Singleton, playerConfig, _playerSpawnService);
            _commandInvoker.ExecuteCommand(command);
            _view.HideView();
        }

        private void OnStartClientClicked() {
            if (_model.SelectedCharacterIndex == -1) {
                _view.ShowNoCharacterSelectedMessage();
                return;
            }

            var playerConfig = _model.GetCharacterConfig(_model.SelectedCharacterIndex);
            var command = new StartClientCommand(NetworkManager.Singleton, playerConfig, _playerSpawnService);
            _commandInvoker.ExecuteCommand(command);
            _view.HideView();
        }

        public void Cleanup() {
            _model.OnCharacterSelected -= OnCharacterSelected;
            _model.OnCharacterHighlighted -= OnCharacterHighlighted;
            _model.OnSelectionConfirmed -= OnSelectionConfirmed;

            _view.OnCharacterButtonClicked -= OnCharacterButtonClicked;
            _view.OnStartHostClicked -= OnStartHostClicked;
            _view.OnStartClientClicked -= OnStartClientClicked;
        }
    }
}