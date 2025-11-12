using System;
using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Services;
using _PrismWars._Scripts.Game.GameManagers;
using _PrismWars._Scripts.Game.GameManagers.Managers;
using _PrismWars._Scripts.Game.Player.Model;
using _PrismWars._Scripts.Game.Services;
using _PrismWars._Scripts.Game.Services.Server;
using _PrismWars._Scripts.Utils;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Infrastructure.Network.Components.Cat {
    public class CatController : NetworkBehaviour, IInitializable<NetworkCatData> {
        [SerializeField] string _catTag = "Cat";
        const string ZONE_TAG = "Zone";
        const string GROUND_LAYER = "Ground";
        
        const int ADD_SCORE_COUNT = 10;
        const float GROUND_TELEPORT_DELAY = 1f;
        
        NetworkScoreService _scoreService;
        CatRespawnService _respawnService;
        SpriteRenderer _catSpriteRenderer;

        Rigidbody2D _catRb;
        
        bool _catIsDespawned = false;
        Timer _groundTimer;

        NetworkVariable<bool> _isOnGround = new NetworkVariable<bool>(false);
        NetworkVariable<bool> _isInZone = new NetworkVariable<bool>(false);

        [NonSerialized]
        public NetworkVariable<NetworkCatData> Data = new();
        
        PlayerElement _playerCaptureElement;
        MapManager _mapManager;
        
        public event Action OnCatDisable;
        
        public void Initialize(NetworkCatData data) {
            if(IsServer) 
                Data.Value = data;
            OnDataChanged(data,data);
        }

        public void SetCatIsDespawned(bool value) {
            _catIsDespawned = value;
            if(value)
                OnCatDisable?.Invoke();
        }
    
        Color GetColorByCatElement(PlayerElement playerElement) {
            return playerElement switch {
                PlayerElement.Fire => Color.red,
                PlayerElement.Ice => Color.blue,
                _ => Color.white
            };
        }

        public override void OnNetworkSpawn() {
            _catRb = GetComponent<Rigidbody2D>();
            _respawnService = ServiceLocator.Singleton.Get<CatRespawnService>();
            _mapManager = ServiceLocator.Singleton.Get<MapManager>();
            _catSpriteRenderer = GetComponent<SpriteRenderer>();
            Data.OnValueChanged += OnDataChanged;
            if (IsServer) {
                _groundTimer = new Timer();
                _groundTimer.OnTimerComplete += OnGroundTimerComplete;
            }
            
            if(!IsServer) return;
            _scoreService = ServiceLocator.Singleton.Get<NetworkScoreService>();
            base.OnNetworkSpawn();
        }

        void OnDataChanged(NetworkCatData previous, NetworkCatData current) {
            GetComponent<SpriteRenderer>().color = GetColorByCatElement(Data.Value.catElement);
            gameObject.name = $"{Data.Value.catElement}Cat";
            gameObject.tag = _catTag;
            gameObject.layer = LayerMask.NameToLayer(Data.Value.catElement.ToString());
        }

        public void FlipX(bool value) {
            FlipXRpc(value);
        }

        [Rpc(SendTo.Everyone)]
        void FlipXRpc(bool value) {
            _catSpriteRenderer.flipX = value;
        }

        void Update() {
            if (!IsServer) return;
            _groundTimer?.Update();
            CheckTeleportConditions();
        }
        
        void OnTriggerEnter2D(Collider2D other) {
            if(_catIsDespawned) return;
            
            if (IsOwner) {
                if(other.CompareTag(ZONE_TAG) && other.gameObject.layer != gameObject.layer) 
                    HandleZoneEnterRpc();
                if(other.CompareTag(ZONE_TAG) && other.gameObject.layer == gameObject.layer) 
                    HandleBaseZoneEnterRpc(true);
                if (other.gameObject.layer == LayerMask.NameToLayer(GROUND_LAYER) && _catRb.bodyType == RigidbodyType2D.Dynamic)  
                    SetOnGroundStateRpc(true);
            }
        }

        void OnTriggerExit2D(Collider2D other) {
            if (IsOwner) {
                if (other.gameObject.layer == LayerMask.NameToLayer(GROUND_LAYER)) 
                    SetOnGroundStateRpc(false);
                if(other.CompareTag(ZONE_TAG) && other.gameObject.layer == gameObject.layer) 
                    HandleBaseZoneEnterRpc(false);
            }
        }

        [Rpc(SendTo.Server)]
        void HandleBaseZoneEnterRpc(bool value) {
            _isInZone.Value = value;
        }

        [Rpc(SendTo.Server)]
        void HandleZoneEnterRpc() {
            if(_catIsDespawned) return;
            
            Debug.Log("Cat entered zone - scoring!");
            _catIsDespawned = true;
            
            var no = GetComponent<NetworkObject>();
            if (no != null) {
                if(!IsOwner) 
                    no.RemoveOwnership();
                _respawnService.CatDespawn(no);
            }
            
            AddScore();
        }

        [Rpc(SendTo.Server)]
        void SetOnGroundStateRpc(bool isOnGround) {
            _isOnGround.Value = isOnGround;
            UpdateTeleportTimer();
        }

        void CheckTeleportConditions() {
            if (!IsServer) return;
            UpdateTeleportTimer();
        }

        void UpdateTeleportTimer() {
            if (!IsServer) return;
            
            bool shouldTeleport = _isOnGround.Value && !_isInZone.Value && !_catIsDespawned;
            
            if (shouldTeleport && !_groundTimer.IsTimerRunning) 
                _groundTimer.StartTimer(GROUND_TELEPORT_DELAY);
            else if (!shouldTeleport && _groundTimer.IsTimerRunning) 
                _groundTimer.StopTimer();
            
        }

        void OnGroundTimerComplete() {
            if (!IsServer) return;
            if (_isOnGround.Value && !_isInZone.Value && !_catIsDespawned) {
                TeleportToBaseRpc();
            }
        }

        [Rpc(SendTo.Owner)]
        void TeleportToBaseRpc() {
            if(_catRb != null) {
                _catRb.linearVelocity = Vector2.zero;
                _catRb.angularVelocity = 0f;
            }

            switch (Data.Value.catElement) {
                default:
                case PlayerElement.Fire:
                    transform.position = _mapManager.Data.catFireSpawnPoint;
                    break;
                case PlayerElement.Ice:
                    transform.position = _mapManager.Data.catIceSpawnPoint;
                    break;
            }

            SetPlayerCaptureElement(PlayerElement.None);
            
            if(IsServer) {
                _isOnGround.Value = false;
                UpdateTeleportTimer();
            }
            else {
                SetOnGroundStateRpc(false);
            }
        }

        public void SetPlayerCaptureElement(PlayerElement element) {
            if(IsServer)
                _playerCaptureElement = element;
            else
                SetPlayerCaptureElementRpc(element);
        }
        [Rpc(SendTo.Server)]
        void SetPlayerCaptureElementRpc(PlayerElement element) {
            _playerCaptureElement = element;
        }
        void AddScore() {
            if(!IsServer) return;
            _scoreService.AddScore(_playerCaptureElement, ADD_SCORE_COUNT);
        }

        public override void OnNetworkDespawn() {
            Data.OnValueChanged -= OnDataChanged;
            if (IsServer && _groundTimer != null) {
                _groundTimer.OnTimerComplete -= OnGroundTimerComplete;
            }
            base.OnNetworkDespawn();
        }
    }
}