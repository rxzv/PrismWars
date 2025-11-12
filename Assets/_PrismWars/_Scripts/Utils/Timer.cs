using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Utils {
    public class Timer{
        float _respawnDuration;

        float _endTime;
        bool _isTimerRunning;
        
        public bool IsTimerRunning => _isTimerRunning;
  
        public System.Action OnTimerComplete;
        
        public void StartTimer(float respawnDuration) {
            _respawnDuration = respawnDuration;
            _endTime = (float)NetworkManager.Singleton.NetworkTimeSystem.ServerTime + _respawnDuration;
            _isTimerRunning = true;
        }

        public void ResetTimer() {
            _respawnDuration = 0;
            _isTimerRunning = false;
        }

        public void StopTimer() {
            _isTimerRunning = false;
        }
        public float GetRemainingTime() {
            if (!_isTimerRunning) return 0;
            
            var remainingTime = _endTime - (float)NetworkManager.Singleton.NetworkTimeSystem.ServerTime;
            return Mathf.Max(0, remainingTime);
        }

        public void Update() {
            if (!_isTimerRunning) return;
            if (!((float)NetworkManager.Singleton.NetworkTimeSystem.ServerTime >=
                  _endTime)) return;
            _isTimerRunning = false;
            OnTimerComplete?.Invoke();
        }
    }
}