using _PrismWars._Scripts.UI;
using _PrismWars._Scripts.Utils;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Game.GameManager {
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkGameManager : NetworkBehaviour {
        NetworkVariable<GameState> _gameState = new();
        
        NetworkGameTimer _networkGameTimer;
        NetworkUIManager _networkUIManager;

        public override void OnNetworkSpawn() {
            _networkGameTimer = ServiceLocator.Singleton.Get<NetworkGameTimer>();
            _networkGameTimer.OnTimerComplete += OnTimerComplete;
            
            if (IsServer) {
                StartGame();
            }
            base.OnNetworkSpawn();
        }

        void StartGame() {
            if (IsServer) {
                _gameState.Value = GameState.Init;
                _networkGameTimer.StartTimerServerRpc(0f);
            }
        }
        
        void OnTimerComplete() {
            if (IsServer) {
                switch (_gameState.Value) {
                    case GameState.Init:
                        _gameState.Value = GameState.SelectCharacter;
                        _networkGameTimer.StartTimerServerRpc(3f);
                        break;
                    case GameState.SelectCharacter:
                        _gameState.Value = GameState.GameStart;
                        _networkGameTimer.StartTimerServerRpc(180f);
                        break;
                } 
            }

            if (IsClient) {
                switch (_gameState.Value) {
                    case GameState.SelectCharacter:
                        SelectCharacter();
                        break;
                    case GameState.GameStart:
                        GameStarted();
                        break;
                }
            }
        }

        void SelectCharacter() {
            _networkUIManager = ServiceLocator.Singleton.Get<NetworkUIManager>();
            _networkUIManager.OnSelectCharacterClientRpc();
        }
        void GameStarted() {
            _networkUIManager = ServiceLocator.Singleton.Get<NetworkUIManager>();
            _networkUIManager.OnGameStartedClientRpc();
        }
        
        enum GameState {
            Init,
            SelectCharacter,
            GameStart,
            GameOver,
        }
    }
}