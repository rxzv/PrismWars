using _PrismWars._Scripts.UI;
using _PrismWars._Scripts.Utils;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Game.GameManagers.Managers {
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkGameManager : NetworkBehaviour {
        const float SELECT_CHARACTER_TIME = 1f;
        float _startGameTime;
        NetworkVariable<GameState> _gameState = new();
        
        NetworkTimer _networkTimer;
        NetworkUIManager _networkUIManager;
        GameModeManager _gameModeManager;

        public override void OnNetworkSpawn() {
            _networkTimer = ServiceLocator.Singleton.Get<NetworkTimer>();
            _networkTimer.OnTimerComplete += OnTimerComplete;
            _gameModeManager = ServiceLocator.Singleton.Get<GameModeManager>();
            _startGameTime = _gameModeManager.Data.timeLimit;
            
            if (IsServer) {
                StartGame();
            }

            if (IsClient) {
                _networkUIManager = ServiceLocator.Singleton.Get<NetworkUIManager>();
            }
            base.OnNetworkSpawn();
        }

        void StartGame() {
            if (IsServer) {
                _gameState.Value = GameState.Init;
                _networkTimer.StartTimerServerRpc(0f);
            }
        }
        
        void OnTimerComplete() {
            if (IsServer) {
                switch (_gameState.Value) {
                    case GameState.Init:
                        _gameState.Value = GameState.SelectCharacter;
                        _networkTimer.StartTimerServerRpc(SELECT_CHARACTER_TIME);
                        break;
                    case GameState.SelectCharacter:
                        _gameState.Value = GameState.GameStart;
                        _networkTimer.StartTimerServerRpc(_startGameTime);
                        break;
                    case GameState.GameStart:
                        _gameState.Value = GameState.GameOver;
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
                    case GameState.GameOver:
                        GameOver();
                        break;
                }
            }
        }

        void SelectCharacter() {
            _networkUIManager.OnSelectCharacterClientRpc();
        }
        void GameStarted() {
            _networkUIManager.OnGameStartedClientRpc();
        }
        void GameOver() {
            _networkUIManager.OnGameOverClientRpc();
        }
        
        enum GameState {
            Init,
            SelectCharacter,
            GameStart,
            GameOver,
        }
    }
}