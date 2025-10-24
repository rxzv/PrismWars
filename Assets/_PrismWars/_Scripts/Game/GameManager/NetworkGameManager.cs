using _PrismWars._Scripts.UI;
using _PrismWars._Scripts.Utils;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Game.GameManager {
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkGameManager : NetworkBehaviour, IService, IInitializable {
        NetworkVariable<GameState> _gameState = new();
        
        NetworkTimer _networkTimer;
        NetworkUIManager _networkUIManager;
        
        public void Initialize() {
            _networkTimer = ServiceLocator.Singleton.Get<NetworkTimer>();
            _networkTimer.OnTimerComplete += OnTimerComplete;
            StartGameRpc();
        }

        [Rpc(SendTo.Server)]
        void StartGameRpc() {
            if (IsServer) {
                _gameState.Value = GameState.Init;
        
                OnChangeGameStateRpc();
            }
        }
        
        void OnTimerComplete() {
            OnChangeGameStateRpc();
        }

        [Rpc(SendTo.Server)]
        void OnChangeGameStateRpc() {
            switch (_gameState.Value) {
                case GameState.Init:
                    StartTimerRpc(10f);
                    SelectCharacterRpc();
                    _gameState.Value = GameState.SelectCharacter;
                    break;
                case GameState.SelectCharacter:
                    StartTimerRpc(180f);
                    GameStartedRpc();
                    _gameState.Value = GameState.GameStart;
                    break;
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        void StartTimerRpc(float time) {
            _networkTimer = ServiceLocator.Singleton.Get<NetworkTimer>();
            _networkTimer.StartTimerServerRpc(time);
        }
        [Rpc(SendTo.ClientsAndHost)]
        void SelectCharacterRpc() {
            _networkUIManager = ServiceLocator.Singleton.Get<NetworkUIManager>();
            _networkUIManager.OnSelectCharacter();
        }
        [Rpc(SendTo.ClientsAndHost)]
        void GameStartedRpc() {
            _networkUIManager = ServiceLocator.Singleton.Get<NetworkUIManager>();
            _networkUIManager.OnGameStarted();
        }
        
        enum GameState {
            Init,
            SelectCharacter,
            GameStart,
            GameOver,
        }
    }
}