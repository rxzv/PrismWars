using UnityEngine;

namespace _PrismWars._Scripts.Game.GameManagers {
    [CreateAssetMenu(fileName = "GameModeData", menuName = "Game/Game Mode Data")]
    public class GameModeData : ScriptableObject {
        public GameMode modeName;
        public int maxPlayers;
        public int scoreLimit = 500;
        public float timeLimit = 180;
    }

    public enum GameMode {
        Deathmatch,
        KingOfTheHill,
        CaptureTheCat,
        BombLoad,
    }
}