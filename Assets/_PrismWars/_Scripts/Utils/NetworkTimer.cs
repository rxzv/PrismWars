using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Services;
using Unity.Netcode;
using UnityEngine;
namespace _PrismWars._Scripts.Utils { 
    public class NetworkTimer : NetworkBehaviour, IService {
        float _timerDuration = 0f;
 
        NetworkVariable<float> _endTime = new NetworkVariable<float>();
        NetworkVariable<bool> _isTimerRunning = new NetworkVariable<bool>();
  
        public System.Action OnTimerComplete;
        
        [Rpc(SendTo.Server)]
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
        
        [Rpc(SendTo.Server)]
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
            if (NetworkManager.Singleton is null || 
                NetworkManager.Singleton.NetworkTimeSystem == null) 
                return 0;
            
            var remainingTime = _endTime.Value - (float)NetworkManager.Singleton.NetworkTimeSystem.ServerTime;
            return Mathf.Max(0, remainingTime);
        }

        void Update() {
            if (NetworkManager.Singleton is null || 
                NetworkManager.Singleton.NetworkTimeSystem == null) 
                return;
            var serverTime = (float)NetworkManager.Singleton.NetworkTimeSystem.ServerTime;
            if(!IsServer || !_isTimerRunning.Value 
                         || !(serverTime >= _endTime.Value))
                return;
            _isTimerRunning.Value = false;
            OnTimerComplete?.Invoke();
        }
    }
}