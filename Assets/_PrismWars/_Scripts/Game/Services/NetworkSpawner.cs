using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Game.Services {
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkSpawner : NetworkBehaviour, IService {
        NetworkVariable<int> _initClients =  new ();

        public void ClientInitialized() {
            ClientInitServerRpc();
        }
        
        [Rpc(SendTo.Server)]
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

        [Rpc(SendTo.Server)]
        void SpawnNetworkObjectServerRpc() {
            SpawnServerGameManager();
        }

        void SpawnServerGameManager() {
            var prefab = Resources.Load<GameObject>("NetworkServicePrefabs/ServerGameManager");
            var instance = Instantiate(prefab);
            instance.GetComponent<NetworkObject>().Spawn();
        }
    }
}