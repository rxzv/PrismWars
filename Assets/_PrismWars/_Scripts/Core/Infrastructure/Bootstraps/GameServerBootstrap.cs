using System.Collections.Generic;
using _PrismWars._Scripts.Core.Patterns.Factory;
using _PrismWars._Scripts.UI;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Infrastructure.Bootstraps {
    [RequireComponent(typeof(NetworkObject))]
    public class GameServerBootstrap : NetworkBehaviour {
        [Header("SceneComponents")]
        [SerializeField] UIManager _uiManager;
        [SerializeField] List<Transform> _fireSpawnPoints;
        [SerializeField] List<Transform> _iceSpawnPoints;
        
        PlayerSpawnService _playerSpawnService;
        
        PlayerFireFactory _playerFireFactory;
        PlayerIceFactory _playerIceFactory;
        
        Transform _playerPrefab;

        public override void OnNetworkSpawn() {
            if (IsServer) {
                Debug.Log("GameServerBootstrap.OnNetworkSpawn");
                ServerServiceInstantiate();
                ServerServiceInitialize();
            }
            ServiceLocator.Singleton.Register(_uiManager);
        }
        void ServerServiceInstantiate() {
            _playerPrefab = Resources.Load<Transform>("Prefabs/Player");
            _playerFireFactory = new PlayerFireFactory(_playerPrefab, _fireSpawnPoints);
            _playerIceFactory = new PlayerIceFactory(_playerPrefab, _iceSpawnPoints);
            
            var servicePrefab = Resources.Load<GameObject>("NetworkServicePrefabs/PlayerSpawnService");
            var serviceGo1 = Instantiate(servicePrefab);
            serviceGo1.GetComponent<NetworkObject>().Spawn(false);
            _playerSpawnService = serviceGo1.GetComponent<PlayerSpawnService>();
            ServiceLocator.Singleton.Register(_playerSpawnService);
            
        }
        void ServerServiceInitialize() {
            _playerSpawnService.Initialize(_playerFireFactory, _playerIceFactory);
        }
    }
}