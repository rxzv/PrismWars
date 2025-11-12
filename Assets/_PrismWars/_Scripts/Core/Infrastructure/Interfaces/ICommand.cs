namespace _PrismWars._Scripts.Core.Infrastructure.Interfaces {
    public interface ICommand {
        void Execute();
        void Undo();
    }
}