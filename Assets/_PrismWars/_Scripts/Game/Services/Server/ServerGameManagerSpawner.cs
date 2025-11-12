using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Services;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Game.Services.Server {
    [RequireComponent(typeof(NetworkObject))]
    public class ServerGameManagerSpawner : NetworkBehaviour, IService {
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
                SpawnNetworkObject();
            }
        }

        void SpawnNetworkObject() {
            if(!IsServer) return;
            SpawnServerGameManager();
        }

        void SpawnServerGameManager() {
            var prefab = Resources.Load<GameObject>("NetworkServicePrefabs/ServerGameManager");
            var instance = Instantiate(prefab);
            instance.GetComponent<NetworkObject>().Spawn();
        }
    }
}