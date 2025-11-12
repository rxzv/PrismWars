using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Services;
using Unity.Netcode;
using UnityEngine;
namespace _PrismWars._Scripts.Utils { 
    public class RespawnTimer : MonoBehaviour, IService {
        float _respawnDuration = 0f;

        float _endTime = 0f;
        bool _isTimerRunning = false;
  
        public System.Action OnTimerComplete;
        
        public void StartRespawnTimer(float respawnDuration) {
            _respawnDuration = respawnDuration;
            StartTimer();
        }
 
        void StartTimer() {
            _endTime = (float)NetworkManager.Singleton.NetworkTimeSystem.ServerTime + _respawnDuration;
            _isTimerRunning = true;
        }

        public void StopTimer() {
            _isTimerRunning = false;
        }
        public float GetRemainingTime() {
            if (!_isTimerRunning) return 0;
            
            float remainingTime = _endTime - (float)NetworkManager.Singleton.NetworkTimeSystem.ServerTime;
            return Mathf.Max(0, remainingTime);
        }

        void Update() {
            if (_isTimerRunning) {
                if ((float)NetworkManager.Singleton.NetworkTimeSystem.ServerTime >= 
                    _endTime) {
                    _isTimerRunning = false;
                    OnTimerComplete?.Invoke();
                }
            }
        }
    }
}