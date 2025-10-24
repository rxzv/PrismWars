using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.UI.Command {
    public class StartClientCommand : ICommand {
        readonly NetworkManager _networkManager;
        readonly PlayerConfig _playerConfig;
        readonly NetworkPlayerSpawnService _networkPlayerSpawnService;

        public StartClientCommand(NetworkManager networkManager, PlayerConfig playerConfig, NetworkPlayerSpawnService _networkPlayerSpawnService) {
            _networkManager = networkManager;
            _playerConfig = playerConfig;
            this._networkPlayerSpawnService = _networkPlayerSpawnService;
        }

        public void Execute() {
            _networkManager.OnClientConnectedCallback += OnClientConnected;
            _networkManager.StartClient();
        }

        void OnClientConnected(ulong clientId) {
            if (clientId == _networkManager.LocalClientId) {
                var networkConfig = _playerConfig.ToNetworkConfig();
                _networkPlayerSpawnService.SpawnPlayerRpc(networkConfig, clientId);
            }
            _networkManager.OnClientConnectedCallback -= OnClientConnected;
        }

        public void Undo() {
            if (_networkManager.IsListening)
                _networkManager.Shutdown();
        }
    }
}