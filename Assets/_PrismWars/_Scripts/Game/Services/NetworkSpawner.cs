using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Game.Services {
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkSpawner : NetworkBehaviour, IService {
        NetworkVariable<int> _initClients =  new NetworkVariable<int>(0);

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
        }
    }
}