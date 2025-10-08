namespace _PrismWars._Scripts.UI.Command {
    public interface ICommand {
        void Execute();
        void Undo();
    }
}