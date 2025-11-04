using System;
using System.Collections.Generic;
using System.Linq;
using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;

namespace _PrismWars._Scripts.Game.Services {
    public class NetworkScoreService : NetworkBehaviour, IService, IInitializable<PlayerElement>{

        [NonSerialized]
        public NetworkVariable<int> IceScore = new();
        [NonSerialized]
        public NetworkVariable<int> FireScore = new();

        [NonSerialized] 
        int _playerScore;
        
        PlayerElement _playerElement;
        
        public PlayerElement PlayerElement => _playerElement;
        public int PlayerScore => _playerScore;

        public void Initialize(PlayerElement data) {
            _playerElement = data;
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddScoreServerRpc(PlayerElement playerElement, int score) {
            if (score <= 0) return;
            switch(playerElement) {
                default:
                case PlayerElement.Ice:
                    IceScore.Value += score;
                    break;
                case PlayerElement.Fire:
                    FireScore.Value += score;
                    break;
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