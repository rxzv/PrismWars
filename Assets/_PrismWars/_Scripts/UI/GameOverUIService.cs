using System.Collections.Generic;
using System.Linq;
using _PrismWars._Scripts.Game.Services;
using _PrismWars._Scripts.UI.Model;
using _PrismWars._Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _PrismWars._Scripts.UI {
    public class GameOverUIService : MonoBehaviour, IService, IInitializable {
        [SerializeField] TextMeshProUGUI _teamWin;
        [SerializeField] TextMeshProUGUI _youTeamCount;
        [SerializeField] TextMeshProUGUI _youCount;
        [SerializeField] Button _lobbyBtn;

        const string LOBBY_SCENE_NAME = "Lobby_Debug";
        
        NetworkScoreService _networkScoreService;
        NetworkChangeScene _networkChangeScene;

        public void Initialize() {
            _networkScoreService = ServiceLocator.Singleton.Get<NetworkScoreService>();
            _networkChangeScene = ServiceLocator.Singleton.Get<NetworkChangeScene>();
            _lobbyBtn.onClick.AddListener(() => _networkChangeScene.ChangeScene(LOBBY_SCENE_NAME));
        }

        public void GameOver() {
            var winTeam = MaxScore(out var max);
            GameOverChangeText(max, winTeam);
        }

        void GameOverChangeText(int maxScore, PlayerElement winTeam) {
            _teamWin.text = $"{winTeam.ToString()} team win! Score: {maxScore}.";
            _youTeamCount.text = $"You team: {_networkScoreService.PlayerElement.ToString()}. Score: {_networkScoreService.GetTeamScores()}";
            _youCount.text = $"You score: {_networkScoreService.PlayerScore}.";
        }

        PlayerElement MaxScore(out int max) {
            Dictionary<PlayerElement, int> scoresDictionary = _networkScoreService.GetAllTeamScores();
            var scores = new int[scoresDictionary.Count];
            for (int i = 0; i < scoresDictionary.Count; i++) {
                scores[i] = scoresDictionary.ElementAt(i).Value;
            }
            max = scores.Max();
            var index = scoresDictionary.Values.ToList().IndexOf(max);
            return scoresDictionary.ElementAt(index).Key;
        }
        
        public void HideView() => gameObject.SetActive(false);
        public void ShowView() => gameObject.SetActive(true);

        void OnDestroy() {
            _lobbyBtn.onClick.RemoveAllListeners();
        }

    }
}