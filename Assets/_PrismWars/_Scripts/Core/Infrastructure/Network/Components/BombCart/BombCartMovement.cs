using System;
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
        
        PlayerElement _targetElement;
        Rigidbody2D _rigidbody;
        NetworkTransform _networkTransform;
        BombCartStateMachine _stateMachine;
        int _lastFireWaypointIndex;
        int _lastIceWaypointIndex;
        
        public PlayerElement CurrentTargetElement => _targetElement;
        List<Vector3> CurrentWaypoints => _targetElement == PlayerElement.Fire ? 
            _fireToIceWaypoints : _iceToFireWaypoints;
        int CurrentIndexWaypoints => _targetElement == PlayerElement.Fire ? 
            _lastFireWaypointIndex : _lastIceWaypointIndex;

        public override void OnNetworkSpawn() {
            _rigidbody = GetComponent<Rigidbody2D>();
            _stateMachine = GetComponent<BombCartStateMachine>();
            _networkTransform = GetComponent<NetworkTransform>();
            _rigidbody.bodyType = RigidbodyType2D.Dynamic;
            _lastFireWaypointIndex = 0;
            _lastIceWaypointIndex = 0;

            if(_networkTransform == null) return;
            _networkTransform.Interpolate = true;
            _networkTransform.SyncPositionX = true;
            _networkTransform.SyncPositionY = true;
            _networkTransform.SyncRotAngleZ = true;
        }

        public void SetTargetElement(PlayerElement element) {
            if (!IsServer) return;
            _targetElement = element;
        }

        public void Move() {
            if (!IsServer) return;
            if (CurrentWaypoints == null || CurrentWaypoints.Count == 0) return;
            
            if(_rigidbody.bodyType != RigidbodyType2D.Dynamic) _rigidbody.bodyType = RigidbodyType2D.Dynamic;
            Vector3 currentWaypoint = CurrentWaypoints[CurrentIndexWaypoints];
            var direction = (currentWaypoint - transform.position).normalized;
            
            _rigidbody.linearVelocity = direction * _moveSpeed;

            if(!(Vector2.Distance(transform.position, currentWaypoint) < 0.1f)) return;
            if(CurrentIndexWaypoints < CurrentWaypoints.Count - 1) {
                IncrementLastIndex();
            }
            else if(CurrentIndexWaypoints == CurrentWaypoints.Count - 1) 
                _stateMachine.SetState(_stateMachine.InZoneState);
        }

        void IncrementLastIndex() {
            switch (_targetElement) {
                case PlayerElement.Fire:
                    _lastFireWaypointIndex++;
                    break;
                case PlayerElement.Ice:
                    _lastIceWaypointIndex++;
                    break;
                case PlayerElement.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Stop() {
            if (!IsServer) return;
            _rigidbody.bodyType = RigidbodyType2D.Static;
            _rigidbody.linearVelocity = Vector2.zero;
        }
    }
}