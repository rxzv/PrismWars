using System;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Game.GameManager {
    [RequireComponent(typeof(NetworkObject))]
    public class ServerGameManager : NetworkBehaviour, IService, IInitializable {
        public event Action OnGameStarted;
        public event Action OnSelectCharacter;

        public void Initialize() {
            OnSelectCharacter?.Invoke();
        }
        
        void GameStarted() {
            OnGameStarted?.Invoke();
        }

    }
}