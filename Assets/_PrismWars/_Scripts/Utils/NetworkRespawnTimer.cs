using Unity.Netcode;
using UnityEngine;
namespace _PrismWars._Scripts.Utils { 
    public class NetworkRespawnTimer : NetworkBehaviour, IService {
        float _respawnDuration = 0f;
 
        float _endTime = 0f;
        bool _isTimerRunning = false;
  
        public System.Action OnTimerComplete;
        
        [ServerRpc]
        public void StartRespawnTimerServerRpc(float respawnDuration) {
            _respawnDuration = respawnDuration;
            StartTimer();
        }
 
        void StartTimer() {
            if (!IsServer) return;

            _endTime = (float)NetworkManager.Singleton.NetworkTimeSystem.ServerTime + _respawnDuration;
            _isTimerRunning = true;
            Debug.Log("Respawn timer started on server. End time: " + _endTime);
        }
        
        [ServerRpc]
        public void StopTimerServerRpc() {
            StopTimer();
        }

        void StopTimer() {
            if (!IsServer) return;
            _isTimerRunning = false;
            Debug.Log("Respawn timer stopped on server.");
        }
        public float GetRemainingTime() {
            if (!_isTimerRunning) return 0;
            
            // Calculate the remaining time based on the network time
            float remainingTime = _endTime - (float)NetworkManager.Singleton.NetworkTimeSystem.ServerTime;
            return Mathf.Max(0, remainingTime); // Ensure it's not negative
        }

        void Update() {
            if (IsServer) {
                if (_isTimerRunning) {
                    // Check if the timer has completed on the server
                    if ((float)NetworkManager.Singleton.NetworkTimeSystem.ServerTime >= 
                        _endTime) {
                        _isTimerRunning = false;
                        OnTimerComplete?.Invoke();
                        Debug.Log("Respawn timer completed on server.");
                    }
                }
            }
            else {
                // For clients, just display the remaining time
                if (_isTimerRunning) {
                    float remainingTime = GetRemainingTime();
                }
            }
        }
    }
}