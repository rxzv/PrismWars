using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Infrastructure.Network.Components.BombCart {
    public class BombCartStateMachine : NetworkBehaviour {
        BombCartBaseState _currentState;
        
        [SerializeField] NetworkVariable<BombCartState> _currentStateType = new();
        
        public NetworkVariable<BombCartState> CurrentStateType => _currentStateType;
        
        public BombCartOfRestState OfRestState { get; private set; }
        public BombCartOfMotionState OfMotionState { get; private set; }
        public BombCartInZoneState InZoneState { get; private set; }

        public override void OnNetworkSpawn() {
            if (IsServer) {
                OfRestState = new BombCartOfRestState(this);
                OfMotionState = new BombCartOfMotionState(this);
                InZoneState = new BombCartInZoneState(this);
                
                SetState(OfRestState);
            }
        }

        public void SetState(BombCartBaseState newState) {
            if (!IsServer) return;
            
            _currentState?.Exit();
            _currentState = newState;
            _currentStateType.Value = newState.StateType;
            _currentState.Enter();
        }

        void Update() {
            if (!IsServer) return;
            _currentState?.Update();
        }

        void FixedUpdate() {
            if (!IsServer) return;
            _currentState?.FixedUpdate();
        }
    }
}