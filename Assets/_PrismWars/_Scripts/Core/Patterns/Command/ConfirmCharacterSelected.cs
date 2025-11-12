using _PrismWars._Scripts.Core.Infrastructure.Interfaces;
using _PrismWars._Scripts.Game.Player.Model;
using _PrismWars._Scripts.Game.Services.Server;
using Unity.Netcode;

namespace _PrismWars._Scripts.Core.Patterns.Command {
    public class ConfirmCharacterSelected : ICommand {
        readonly PlayerConfig _playerConfig;
        readonly NetworkPlayerSpawnService _networkPlayerSpawnService;

        public ConfirmCharacterSelected(PlayerConfig playerConfig) {
            _playerConfig = playerConfig;
            _networkPlayerSpawnService = ServiceLocator.Singleton.Get<NetworkPlayerSpawnService>();
        }

        public void Execute() {
            NetworkPlayerData networkPlayerData = _playerConfig.ToNetworkConfig();
            _networkPlayerSpawnService.SpawnPlayerRpc(networkPlayerData, NetworkManager.Singleton.LocalClientId);
        }

        public void Undo() {
            // 
        }
    }
}
