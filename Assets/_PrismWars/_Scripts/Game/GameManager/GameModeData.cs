using UnityEngine;

namespace _PrismWars._Scripts.Game.GameManager {
    [CreateAssetMenu(fileName = "GameModeData", menuName = "Game/Game Mode Data")]
    public class GameModeData : ScriptableObject {
        public GameMode modeName;
        public int maxPlayers;
        public int scoreLimit;
        public float timeLimit;
    }

    public enum GameMode {
        Deathmatch,
        KingOfTheHill,
        CaptureTheCat,
        BombLoad,
    }
}