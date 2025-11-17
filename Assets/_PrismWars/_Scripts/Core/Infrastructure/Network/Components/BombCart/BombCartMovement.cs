using System.Collections.Generic;
using _PrismWars._Scripts.Game.Player.Model;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Infrastructure.Network.Components.BombCart {
    public class BombCartMovement : NetworkBehaviour {
        [Header("Movement Settings")]
        [SerializeField] float _moveSpeed = 3f;
        [SerializeField] List<Vector3> _fireToIceWaypoints;
        [SerializeField] List<Vector3> _iceToFireWaypoints;
        
        [SerializeField]NetworkVariable<PlayerElement> _targetElement = new();
        [SerializeField]NetworkVariable<int> _currentWaypointIndex = new();
        Rigidbody2D _rigidbody;
        NetworkTransform _networkTransform;
        BombCartStateMachine _stateMachine;
        
        public PlayerElement CurrentTargetElement => _targetElement.Value;
        List<Vector3> CurrentWaypoints => _targetElement.Value == PlayerElement.Fire ? 
            _fireToIceWaypoints : _iceToFireWaypoints;

        public override void OnNetworkSpawn() {
            _rigidbody = GetComponent<Rigidbody2D>();
            _stateMachine = GetComponent<BombCartStateMachine>();
            _networkTransform = GetComponent<NetworkTransform>();
            _rigidbody.bodyType = RigidbodyType2D.Dynamic;

            if(_networkTransform == null) return;
            _networkTransform.Interpolate = true;
            _networkTransform.SyncPositionX = true;
            _networkTransform.SyncPositionY = true;
        }

        public void SetTargetElement(PlayerElement element) {
            if (!IsServer) return;
            _targetElement.Value = element;
            _currentWaypointIndex.Value = 0;
        }

        public void Move() {
            if (!IsServer) return;
            if (CurrentWaypoints == null || CurrentWaypoints.Count == 0) return;
            
            if(_rigidbody.bodyType != RigidbodyType2D.Dynamic) _rigidbody.bodyType = RigidbodyType2D.Dynamic;
            var currentWaypoint = CurrentWaypoints[_currentWaypointIndex.Value];
            var direction = (currentWaypoint - transform.position).normalized;
            
            _rigidbody.linearVelocity = direction * _moveSpeed;

            if(!(Vector2.Distance(transform.position, currentWaypoint) < 0.1f)) return;
            if (_currentWaypointIndex.Value < CurrentWaypoints.Count - 1) 
                _currentWaypointIndex.Value++;
            else if(_currentWaypointIndex.Value == CurrentWaypoints.Count - 1) 
                _stateMachine.SetState(_stateMachine.InZoneState);
        }

        public void Stop() {
            if (!IsServer) return;
            _rigidbody.bodyType = RigidbodyType2D.Static;
            _rigidbody.linearVelocity = Vector2.zero;
        }
    }
}