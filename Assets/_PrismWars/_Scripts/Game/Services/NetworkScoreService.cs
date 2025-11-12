using System;
using System.Collections.Generic;
using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Services;
using _PrismWars._Scripts.Game.GameManagers.Managers;
using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;

namespace _PrismWars._Scripts.Game.Services {
    public class NetworkScoreService : NetworkBehaviour, IService, IInitializable<PlayerElement>{

        [NonSerialized]
        public NetworkVariable<int> IceScore = new();
        [NonSerialized]
        public NetworkVariable<int> FireScore = new();

        int _scoreLimit = 100;
        GameModeManager _gameModeManager;

        int _playerScore;
        
        PlayerElement _playerElement;
        
        public PlayerElement PlayerElement => _playerElement;
        public int PlayerScore => _playerScore;

        public void Initialize(PlayerElement data) {
            _playerElement = data;
            _gameModeManager = ServiceLocator.Singleton.Get<GameModeManager>();
            _scoreLimit = _gameModeManager.Data.scoreLimit;
        }

        public void AddScore(PlayerElement element, int score) {
            if(!IsServer) return;
            if (score <= 0 || element == PlayerElement.None) return;
            switch(element) {
                default:
                case PlayerElement.Ice:
                    if(IceScore.Value < _scoreLimit){
                        IceScore.Value += score; 
                    }
                    break;
                case PlayerElement.Fire:
                    if(FireScore.Value < _scoreLimit){
                        FireScore.Value += score;
                    }
                    break;
            }
        }

        [Rpc(SendTo.Server)]
        public void AddScoreForDiedServerRpc(PlayerElement playerElement, int score, ulong killerId) {
            AddScore(playerElement, score);
            AddScoreForKillerClientRpc(score, killerId);
        }

        [Rpc(SendTo.ClientsAndHost)]
        void AddScoreForKillerClientRpc(int score, ulong killerId) {
            if (NetworkManager.Singleton.LocalClientId == killerId && score > 0) {
                _playerScore += score;
            }
        }
        
        public int GetTeamScores() => _playerElement switch {
                PlayerElement.Fire => FireScore.Value,
                PlayerElement.Ice => IceScore.Value,
                _ => 0 };
        
        public Dictionary<PlayerElement, int> GetAllTeamScores() {
            return new Dictionary<PlayerElement, int>
            {
                { PlayerElement.Fire, FireScore.Value },
                { PlayerElement.Ice, IceScore.Value }
            };
        }
    }
}