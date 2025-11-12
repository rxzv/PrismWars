using _PrismWars._Scripts.Core.Infrastructure.Interfaces;
using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;

namespace _PrismWars._Scripts.UI.Command {
    public class StartHostCommand : ICommand
    {
        readonly NetworkManager _networkManager;
        readonly PlayerConfig _playerConfig;
        readonly NetworkPlayerSpawnService _networkPlayerSpawnService;

        public StartHostCommand(NetworkManager networkManager, PlayerConfig playerConfig, NetworkPlayerSpawnService _networkPlayerSpawnService) {
            _networkManager = networkManager;
            _playerConfig = playerConfig;
            this._networkPlayerSpawnService = _networkPlayerSpawnService;
        }

        public void Execute() {
            _networkManager.OnServerStarted += OnHostStarted;
            _networkManager.StartHost();
        }

        void OnHostStarted() {
            var networkConfig = _playerConfig.ToNetworkConfig();
            // _playerSpawnService.SpawnPlayerServerRpc(networkConfig);
            _networkManager.OnServerStarted -= OnHostStarted;
        }

        public void Undo() 
        {
            if (_networkManager.IsListening)
                _networkManager.Shutdown();
        }
    }
}