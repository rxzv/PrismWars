using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Services;
using Unity.Netcode;
using UnityEngine;
namespace _PrismWars._Scripts.Utils { 
    public class NetworkGameTimer : NetworkBehaviour, IService {
        float _timerDuration = 0f;
 
        NetworkVariable<float> _endTime = new NetworkVariable<float>();
        NetworkVariable<bool> _isTimerRunning = new NetworkVariable<bool>();
  
        public System.Action OnTimerComplete;
        
        [ServerRpc]
        public void StartTimerServerRpc(float timerDuration) {
            _timerDuration = timerDuration;
            StartTimer();
        }
 
        void StartTimer() {
            if (!IsServer) return;

            _endTime.Value = (float)NetworkManager.Singleton.NetworkTimeSystem.ServerTime + _timerDuration;
            _isTimerRunning.Value = true;
            Debug.Log("Timer started on server. End time: " + _endTime.Value);
        }
        
        [ServerRpc]
        public void StopTimerServerRpc() {
            StopTimer();
        }

        void StopTimer() {
            if (!IsServer) return;
            _isTimerRunning.Value = false;
            Debug.Log("Timer stopped on server.");
        }
        public float GetRemainingTime() {
            if (!_isTimerRunning.Value) return 0;
            
            float remainingTime = _endTime.Value - (float)NetworkManager.Singleton.NetworkTimeSystem.ServerTime;
            return Mathf.Max(0, remainingTime);
        }

        void Update() {
            if (IsServer) {
                if (_isTimerRunning.Value) {
                    if ((float)NetworkManager.Singleton.NetworkTimeSystem.ServerTime >= _endTime.Value) {
                        _isTimerRunning.Value = false;
                        OnTimerComplete?.Invoke();
                    }
                }
            }
        }
    }
}