using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;

namespace _PrismWars._Scripts.UI.Command {
    public class StartClientCommand : ICommand {
        private readonly NetworkManager _networkManager;
        private readonly PlayerConfig _playerConfig;

        public StartClientCommand(NetworkManager networkManager, PlayerConfig playerConfig) {
            _networkManager = networkManager;
            _playerConfig = playerConfig;
        }

        public void Execute() {
            ServiceLocator.Current.Get<PlayerSpawnService>().SetPlayerConfig(_playerConfig);
            _networkManager.StartClient();
        }

        public void Undo() {
            // _networkManager.StopClient();
        }
    }
}