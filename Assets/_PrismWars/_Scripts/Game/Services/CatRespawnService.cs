using System;
using System.Collections.Generic;
using _PrismWars._Scripts.Core.Infrastructure.Network.Components;
using _PrismWars._Scripts.UI.Model;
using _PrismWars._Scripts.Utils;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Game.Services {
    public class CatRespawnService : NetworkBehaviour, IService, IInitializable<Transform, Transform> {
        const float CAT_TIME_TO_RESPAWN = 3f;
        Transform _fireSpawnPoint;
        Transform _iceSpawnPoint;
        Timer _respawnTimer;
        
        Queue<NetworkObjectReference> _catToRespawn = new();

        public void Initialize(Transform fireSpawnPoint, Transform iceSpawnPoint) {
            _fireSpawnPoint = fireSpawnPoint;
            _iceSpawnPoint = iceSpawnPoint;
            _respawnTimer = new Timer();
        }
        
        [Rpc(SendTo.Server)]
        public void CatDespawnRpc(NetworkObjectReference catRef) {
            _catToRespawn.Enqueue(catRef);
            CatDespawnRpc(catRef, false);
            CatStartRespawnTimer();
        }

        void Update() {
            if(!IsServer) return;
            _respawnTimer?.Update();
        }

        void CatStartRespawnTimer() {
            if(!IsServer) return;
            _respawnTimer.OnTimerComplete += RespawnCat;
            _respawnTimer.StartTimer(CAT_TIME_TO_RESPAWN);
        }
        
        void RespawnCat() {
            if(!IsServer) return;
            var catRef = _catToRespawn.Dequeue();
            catRef.TryGet(out NetworkObject cat);
            cat.TryGetComponent(out CatController catController);
            if (catController == null) return;
            switch (catController.Data.Value.catElement) {
                default:
                case PlayerElement.Fire:
                    cat.transform.position = _fireSpawnPoint.position;
                    break;
                case PlayerElement.Ice:
                    cat.transform.position = _iceSpawnPoint.position;
                    break;
            }

            cat.GetComponent<CatController>()?.SetCatIsDespawned(true);
            CatDespawnRpc(cat, true);
        }
        
        [Rpc(SendTo.ClientsAndHost)]
        void CatDespawnRpc(NetworkObjectReference catRef, bool active) {
            catRef.TryGet(out NetworkObject networkObject);
            if(networkObject == null) return;
            networkObject.gameObject.SetActive(active);
            networkObject.GetComponent<CatController>()?.SetCatIsDespawned(active);
        }
    }
}