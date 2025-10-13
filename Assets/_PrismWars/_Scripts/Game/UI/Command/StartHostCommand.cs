using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;

namespace _PrismWars._Scripts.UI.Command {
    public class StartHostCommand : ICommand
    {
        readonly NetworkManager _networkManager;
        readonly PlayerConfig _playerConfig;
        readonly PlayerSpawnService _playerSpawnService;

        public StartHostCommand(NetworkManager networkManager, PlayerConfig playerConfig, PlayerSpawnService playerSpawnService) {
            _networkManager = networkManager;
            _playerConfig = playerConfig;
            _playerSpawnService = playerSpawnService;
        }

        public void Execute() {
            _networkManager.OnServerStarted += OnHostStarted;
            _networkManager.StartHost();
        }

        void OnHostStarted() {
            var networkConfig = _playerConfig.ToNetworkConfig();
            _playerSpawnService.SpawnPlayerServerRpc(networkConfig);
            _networkManager.OnServerStarted -= OnHostStarted;
        }

        public void Undo() 
        {
            if (_networkManager.IsListening)
                _networkManager.Shutdown();
        }
    }
}