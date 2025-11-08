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

        int _playerScore;
        
        PlayerElement _playerElement;
        
        public PlayerElement PlayerElement => _playerElement;
        public int PlayerScore => _playerScore;

        public void Initialize(PlayerElement data) {
            _playerElement = data;
        }

        public void AddScore(PlayerElement element, int score) {
            if(!IsServer) return;
            if (score <= 0 || element == PlayerElement.None) return;
            switch(element) {
                default:
                case PlayerElement.Ice:
                    IceScore.Value += score;
                    break;
                case PlayerElement.Fire:
                    FireScore.Value += score;
                    break;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddScoreForDiedServerRpc(PlayerElement playerElement, int score, ulong killerId) {
            AddScore(playerElement, score);
            AddScoreForKillerClientRpc(score, killerId);
        }

        [ClientRpc(RequireOwnership = false)]
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