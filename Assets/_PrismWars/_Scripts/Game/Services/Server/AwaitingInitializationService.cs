using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Services;
using _PrismWars._Scripts.Utils;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Game.Services.Server {
    [RequireComponent(typeof(NetworkObject))]
    public class AwaitingInitializationService : NetworkBehaviour, IService, IInitializable<string> {
        NetworkVariable<int> _initBootstrapClients = new();
        NetworkVariable<int> _initGameClients = new();

        string _gameSceneName = "Game";
        
        public void Initialize(string gameSceneName) {
            _gameSceneName = gameSceneName;
        }

        public void ClientGameInitialized() {
            ClientInitServerRpc();
        }
        public void ClientBootstrapInitialized() {
            ClientBootstrapInitServerRpc();
        }
        [Rpc(SendTo.Server)]
        void ClientBootstrapInitServerRpc() {
            _initBootstrapClients.Value++;
        }
        
        [Rpc(SendTo.Server)]
        void ClientInitServerRpc() {
            _initGameClients.Value++;
        }

        public override void OnNetworkSpawn() {
            if (IsServer) {
                _initGameClients.OnValueChanged += ClientGameInitChanged;
                _initBootstrapClients.OnValueChanged += ClientBootstrapInitChanged;
            }
            base.OnNetworkSpawn();
        }

        void ClientBootstrapInitChanged(int previousValue, int newValue) {
            if (IsServer && newValue == GameLobbyManager.Instance.MaxPlayers) {
                var networkChangeScene = ServiceLocator.Singleton.Get<NetworkChangeScene>();
                networkChangeScene.ChangeScene(_gameSceneName);
            }
        }
        void ClientGameInitChanged(int previousValue, int newValue) {
            if (IsServer && newValue == GameLobbyManager.Instance.MaxPlayers) {
                SpawnServerGameManager();
            }
        }

        void SpawnServerGameManager() {
            var prefab = Resources.Load<GameObject>("NetworkServicePrefabs/ServerGameManager");
            var instance = Instantiate(prefab);
            instance.GetComponent<NetworkObject>().Spawn();
        }

    }
}