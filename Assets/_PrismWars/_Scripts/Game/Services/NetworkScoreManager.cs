using System;
using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;

namespace _PrismWars._Scripts.Game.Services {
    public class NetworkScoreManager : NetworkBehaviour, IService {

        [NonSerialized]
        public NetworkVariable<int> IceScore = new();
        [NonSerialized]
        public NetworkVariable<int> FireScore = new();

        [ServerRpc]
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

    }
}