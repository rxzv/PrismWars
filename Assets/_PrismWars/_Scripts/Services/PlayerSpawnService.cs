using System;
using _PrismWars._Scripts.Player;
using R3;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts {
    public class PlayerSpawnService : NetworkBehaviour, IService,IDisposable, IInitializable<Transform> {

        Transform _playerPrefab;

        public readonly Subject<(ulong clientId, NetworkObjectReference playerRef)> OnPlayerSpawned = new();
        readonly CompositeDisposable _disposables = new();
        
        public void Initialize(Transform playerPrefab) {
            _playerPrefab = playerPrefab;
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
            NetworkObject networkObject = currentPlayer.GetComponent<NetworkObject>();
            networkObject.SpawnWithOwnership(clientId, true);
            SpawnPlayerRpc(networkObject, clientId);
        }

        [Rpc(SendTo.ClientsAndHost)]
        void SpawnPlayerRpc(NetworkObjectReference transform, ulong clientId) =>
            OnPlayerSpawned?.OnNext((clientId,transform));

        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}