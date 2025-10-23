using _PrismWars._Scripts.UI.Model;

namespace _PrismWars._Scripts.UI.Command {
    public class ConfirmCharacterSelected : ICommand {
        readonly PlayerConfig _playerConfig;
        readonly NetworkPlayerSpawnService _networkPlayerSpawnService;

        public ConfirmCharacterSelected(PlayerConfig playerConfig) {
            _playerConfig = playerConfig;
            _networkPlayerSpawnService = ClientServiceLocator.Singleton.Get<NetworkPlayerSpawnService>();
        }

        public void Execute() {
            NetworkPlayerData networkPlayerData = _playerConfig.ToNetworkConfig();
            _networkPlayerSpawnService.SpawnPlayerServerRpc(networkPlayerData);
        }

        public void Undo() {
            // 
        }
    }
}
