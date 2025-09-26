using _PrismWars._Scripts.Player;
using R3;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts {
    public class PlayerSpawnService : NetworkBehaviour, IService, IInitializable<Transform, PlayerConfig> {

        Transform _playerPrefab;
        PlayerConfig _playerConfig;

        public readonly Subject<(ulong clientId, NetworkObjectReference playerRef)> OnPlayerSpawned = new();
        readonly CompositeDisposable _disposables = new();
        
        public void Initialize(Transform playerPrefab, PlayerConfig playerConfig) {
            _playerPrefab = playerPrefab;
            _playerConfig = playerConfig;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }

        void OnClientConnected(ulong clientId) {
            if (IsServer) {
                Observable.NextFrame()
                    .Subscribe(_ => SpawnPlayer(clientId))
                    .AddTo(_disposables);
            }
        }
        void SpawnPlayer(ulong clientId) {
            if (!IsServer) return;
            
            var currentPlayer = Instantiate(_playerPrefab);
            currentPlayer.GetComponent<PlayerController>().Initialize(_playerConfig);
            NetworkObject networkObject = currentPlayer.GetComponent<NetworkObject>();
            networkObject.SpawnWithOwnership(clientId, true);
            SpawnPlayerRpc(networkObject, clientId);
        }

        [Rpc(SendTo.ClientsAndHost)]
        void SpawnPlayerRpc(NetworkObjectReference transform, ulong clientId) =>
            OnPlayerSpawned?.OnNext((clientId,transform));

        public void Dispose()
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            _disposables?.Dispose();
        }
    }
}