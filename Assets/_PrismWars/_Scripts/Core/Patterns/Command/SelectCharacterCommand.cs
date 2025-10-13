using _PrismWars._Scripts.UI.Controller;

namespace _PrismWars._Scripts.UI.Command {
    public class SelectCharacterCommand : ICommand
    {
        readonly CharacterSelectionController _controller;
        readonly int _characterIndex;
        int _previousSelection;

        public SelectCharacterCommand(CharacterSelectionController controller, int characterIndex) {
            _controller = controller;
            _characterIndex = characterIndex;
        }

        public void Execute() {
            _previousSelection = _controller.GetSelectedCharacterIndex();
            _controller.SelectCharacter(_characterIndex);
        }

        public void Undo() {
            if (_previousSelection != -1) {
                _controller.SelectCharacter(_previousSelection);
            }
        }
    }
}