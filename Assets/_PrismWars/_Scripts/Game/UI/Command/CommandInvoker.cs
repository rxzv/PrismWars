using System.Collections.Generic;

namespace _PrismWars._Scripts.UI.Command {
    public class CommandInvoker
    {
        private readonly Stack<ICommand> _commandHistory = new Stack<ICommand>();

        public void ExecuteCommand(ICommand command) {
            command.Execute();
            _commandHistory.Push(command);
        }

        public void UndoLastCommand() {
            if (_commandHistory.Count > 0) {
                var command = _commandHistory.Pop();
                command.Undo();
            }
        }

        public void ClearHistory() {
            _commandHistory.Clear();
        }
    }
}