using _PrismWars._Scripts.Core.Infrastructure.Network.Components.Cat;
using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Game.Services {
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkSpawner : NetworkBehaviour, IService {
        NetworkVariable<int> _initClients =  new NetworkVariable<int>(0);
        ServerCatSpawnService _serverCatSpawnService;

        public void ClientInitialized() {
            ClientInitServerRpc();
        }
        
        [ServerRpc(RequireOwnership = false)]
        void ClientInitServerRpc() {
            _initClients.Value++;
        }

        public override void OnNetworkSpawn() {
            if (IsServer) {
                _initClients.OnValueChanged += ClientInitChanged;
            }
            base.OnNetworkSpawn();
        }


        void ClientInitChanged(int previousValue, int newValue) {
            if (IsServer && newValue == GameLobbyManager.Instance.MaxPlayers) {
                SpawnNetworkObjectServerRpc();
            }
        }

        [ServerRpc]
        void SpawnNetworkObjectServerRpc() {
            var prefab = Resources.Load<GameObject>("NetworkServicePrefabs/ServerGameManager");
            var instance = Instantiate(prefab);
            instance.GetComponent<NetworkObject>().Spawn();
            
            prefab = Resources.Load<GameObject>("NetworkServicePrefabs/HillCaptureComponent");
            instance = Instantiate(prefab);
            instance.GetComponent<NetworkObject>().Spawn();
            
            _serverCatSpawnService = ServiceLocator.Singleton.Get<ServerCatSpawnService>();
            _serverCatSpawnService.SpawnCatRpc(
                new NetworkCatData{catElement = PlayerElement.Fire}, 
                NetworkManager.Singleton.LocalClientId);
            _serverCatSpawnService.SpawnCatRpc(
                new NetworkCatData{catElement = PlayerElement.Ice}, 
                NetworkManager.Singleton.LocalClientId);
        }
    }
}