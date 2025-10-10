using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.UI.Command {
    public class StartClientCommand : ICommand {
        readonly NetworkManager _networkManager;
        readonly PlayerConfig _playerConfig;
        readonly PlayerSpawnService _playerSpawnService;

        public StartClientCommand(NetworkManager networkManager, PlayerConfig playerConfig, PlayerSpawnService playerSpawnService) {
            _networkManager = networkManager;
            _playerConfig = playerConfig;
            _playerSpawnService = playerSpawnService;
        }

        public void Execute() {
            _networkManager.OnClientConnectedCallback += OnClientConnected;
            _networkManager.StartClient();
        }

        void OnClientConnected(ulong clientId) {
            if (clientId == _networkManager.LocalClientId) {
                var networkConfig = _playerConfig.ToNetworkConfig();
                _playerSpawnService.SpawnPlayerServerRpc(networkConfig);
            }
            _networkManager.OnClientConnectedCallback -= OnClientConnected;
        }

        public void Undo() {
            if (_networkManager.IsListening)
                _networkManager.Shutdown();
        }
    }
}