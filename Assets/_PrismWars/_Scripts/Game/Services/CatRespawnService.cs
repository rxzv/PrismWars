using System;
using System.Collections.Generic;
using _PrismWars._Scripts.Core.Infrastructure.Network.Components;
using _PrismWars._Scripts.UI.Model;
using _PrismWars._Scripts.Utils;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Game.Services {
    public class CatRespawnService : NetworkBehaviour, IService, IInitializable<Vector3, Vector3> {
        const float CAT_TIME_TO_RESPAWN = 3f;
        Vector3 _fireSpawnPoint;
        Vector3 _iceSpawnPoint;
        Timer _respawnTimer;
        
        Queue<NetworkObjectReference> _catToRespawn = new();

        public void Initialize(Vector3 fireSpawnPoint, Vector3 iceSpawnPoint) {
            _fireSpawnPoint = fireSpawnPoint;
            _iceSpawnPoint = iceSpawnPoint;
            _respawnTimer = new Timer();
        }
        
        [Rpc(SendTo.Server)]
        public void CatDespawnRpc(NetworkObjectReference catRef) {
            _catToRespawn.Enqueue(catRef);
            SetCatActiveStateRpc(catRef, false);
            CatStartRespawnTimer();
        }

        void Update() {
            if(!IsServer) return;
            _respawnTimer?.Update();
        }

        void CatStartRespawnTimer() {
            if(!IsServer) return;
            if (_respawnTimer.GetRemainingTime() > 0) return; 
            
            _respawnTimer.OnTimerComplete += RespawnCat;
            _respawnTimer.StartTimer(CAT_TIME_TO_RESPAWN);
        }
        
        void RespawnCat() {
            if(!IsServer) return;
            if (_catToRespawn.Count == 0) return;
            
            var catRef = _catToRespawn.Dequeue();
            catRef.TryGet(out NetworkObject cat);
            if (cat == null) return;
            
            cat.TryGetComponent(out CatController catController);
            if (catController == null) return;

            var rb = cat.GetComponent<Rigidbody2D>();
            if (rb != null) {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            switch (catController.Data.Value.catElement) {
                default:
                case PlayerElement.Fire:
                    cat.transform.position = _fireSpawnPoint;
                    break;
                case PlayerElement.Ice:
                    cat.transform.position = _iceSpawnPoint;
                    break;
            }

            catController.SetCatIsDespawned(false);
            catController.SetPlayerCaptureElementRpc(PlayerElement.None);
            
            SetCatActiveStateRpc(cat, true);
        }
        
        [Rpc(SendTo.ClientsAndHost)]
        void SetCatActiveStateRpc(NetworkObjectReference catRef, bool active) {
            if (catRef.TryGet(out NetworkObject networkObject) && networkObject != null)
            {
                networkObject.gameObject.SetActive(active);
                var catController = networkObject.GetComponent<CatController>();
                if (catController != null) {
                    catController.SetCatIsDespawned(!active);
                    
                    if (active) {
                        var rb = networkObject.GetComponent<Rigidbody2D>();
                        if (rb != null) {
                            rb.linearVelocity = Vector2.zero;
                            rb.angularVelocity = 0f;
                        }
                    }
                }
            }
        }

        void OnDestroy() {
            _respawnTimer.OnTimerComplete -= RespawnCat;
        }
    }
}