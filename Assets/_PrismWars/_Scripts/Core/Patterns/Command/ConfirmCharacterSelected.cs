using _PrismWars._Scripts.UI.Model;

namespace _PrismWars._Scripts.UI.Command {
    public class ConfirmCharacterSelected : ICommand {
        readonly PlayerConfig _playerConfig;
        readonly PlayerSpawnService _playerSpawnService;

        public ConfirmCharacterSelected(PlayerConfig playerConfig) {
            _playerConfig = playerConfig;
            _playerSpawnService = ServiceLocator.Current.Get<PlayerSpawnService>();
        }

        public void Execute() {
            NetworkPlayerData networkPlayerData = _playerConfig.ToNetworkConfig();
            _playerSpawnService.SpawnPlayerServerRpc(networkPlayerData);
        }

        public void Undo() {
            // 
        }
    }
}
