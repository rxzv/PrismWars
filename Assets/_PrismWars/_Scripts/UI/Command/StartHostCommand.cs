using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;

namespace _PrismWars._Scripts.UI.Command {
    public class StartHostCommand : ICommand
    {
        readonly NetworkManager _networkManager;
        readonly PlayerConfig _playerConfig;

        public StartHostCommand(NetworkManager networkManager, PlayerConfig playerConfig) {
            _networkManager = networkManager;
            _playerConfig = playerConfig;
        }

        public void Execute() {
            ServiceLocator.Current.Get<PlayerSpawnService>().SetPlayerConfig(_playerConfig);
            _networkManager.StartHost();
        }

        public void Undo() {
            // Отмена запуска хоста (если нужно)
            // _networkManager.StopHost();
        }
    }
}