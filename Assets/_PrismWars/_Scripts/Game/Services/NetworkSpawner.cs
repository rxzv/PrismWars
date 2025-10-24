using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Game.Services {
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkSpawner : NetworkBehaviour {

        async void Start() {
            await Task.Delay(1000);
            if (IsServer) {
                SpawnNetworkObjectServerRpc();
            }
            base.OnNetworkSpawn();
        }

        [ServerRpc]
        void SpawnNetworkObjectServerRpc() {
            var prefab = Resources.Load<GameObject>("NetworkServicePrefabs/ServerGameManager");
            var instance = Instantiate(prefab);
            instance.GetComponent<NetworkObject>().Spawn();
        }
    }
}