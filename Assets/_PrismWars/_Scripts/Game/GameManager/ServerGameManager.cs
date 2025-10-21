using System;
using System.Threading.Tasks;
using _PrismWars._Scripts.Utils;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Game.GameManager {
    [RequireComponent(typeof(NetworkObject))]
    public class ServerGameManager : NetworkBehaviour, IService, IInitializable {
        
        NetworkTimer _networkTimer;
        NetworkVariable<GameState> _gameState = new NetworkVariable<GameState>();
        
        public event Action OnSelectCharacter;
        public event Action OnGameStarted;

        public void Initialize() {
            if(IsServer)
                _gameState.Value = GameState.Init;
            
            _networkTimer = ServiceLocator.Singleton.Get<NetworkTimer>();

            OnGameState();
            _networkTimer.OnTimerComplete += OnGameState;
        }
        
        void OnGameState() {
            switch (_gameState.Value) {
                case GameState.Init:
                    _networkTimer.StartTimerServerRpc(10);
                    OnSelectCharacterRpc();
                    _gameState.Value = GameState.SelectCharacter;
                    break;
                case GameState.SelectCharacter:
                    _networkTimer.StartTimerServerRpc(180);
                    OnGameStartedRpc();
                    _gameState.Value = GameState.GameStart;
                    break;
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        void OnSelectCharacterRpc() {
            OnSelectCharacter?.Invoke();
        }
        [Rpc(SendTo.ClientsAndHost)]
        void OnGameStartedRpc() {
            OnGameStarted?.Invoke();
        }


        enum GameState {
            Init,
            SelectCharacter,
            GameStart,
            GameOver,
        }
    }
}