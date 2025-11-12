using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Services;
using _PrismWars._Scripts.Game.Services;
using _PrismWars._Scripts.Game.Services.Server;
using R3;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace _PrismWars._Scripts.UI.Services.Mono {
    public class GameUIViewService : MonoBehaviour, IService, IInitializable {
        [SerializeField] Slider _healthSlider;
        [SerializeField] TextMeshProUGUI _fireScore;
        [SerializeField] TextMeshProUGUI _iceScore;
        [SerializeField] TextMeshProUGUI _shardCount;
        [SerializeField] GameCursorUIService _gameCursorUIService;
        
        NetworkScoreService _networkScoreService;
        NetworkPlayerSpawnService _networkPlayerSpawnService;
        
        float _maxHealth;
        
        readonly CompositeDisposable _disposables = new();

        public void ShowView() => gameObject.SetActive(true);
        public void HideView() => gameObject.SetActive(false);
        
        public void Initialize() {
            _networkScoreService = ServiceLocator.Singleton.Get<NetworkScoreService>();
            _networkPlayerSpawnService = ServiceLocator.Singleton.Get<NetworkPlayerSpawnService>();
            
            ServiceLocator.Singleton.Register(_gameCursorUIService);
            
            _networkScoreService.FireScore.OnValueChanged += FireScoreChanged;
            _networkScoreService.IceScore.OnValueChanged += IceScoreChanged;
            
            _networkPlayerSpawnService.OnPlayerSpawned
                .Where(tuple => tuple.clientId == NetworkManager.Singleton.LocalClientId)
                .Subscribe(_ => OnPlayerSpawned())
                .AddTo(_disposables);
        }

        void OnPlayerSpawned() {
            GameCursorUIServiceInitialize();
        }

        void GameCursorUIServiceInitialize() {
            _gameCursorUIService.Initialize();
            _gameCursorUIService.Show();
        }

        void FireScoreChanged(int previousValue, int newValue) {
            if (previousValue != newValue && newValue > 0) {
                _fireScore.text = newValue.ToString();
            }
        }
        void IceScoreChanged(int previousValue, int newValue) {
            if (previousValue != newValue && newValue > 0) {
                _iceScore.text = newValue.ToString();
            }
        }

        public void SetMaxHealth(float maxHealth) {
            if (maxHealth > 0) {
                _maxHealth =  maxHealth;
                UpdateHealthBar(maxHealth);
            }
        }

        public void UpdateShardCount(int count) {
            if (count >= 0) {
                _shardCount.text = count.ToString();
            }
        }
        
        public void UpdateHealthBar(float health) {
            if (_healthSlider != null)
                _healthSlider.value = health / _maxHealth;
        }

        void Awake() {
            _fireScore.text = "0";
            _iceScore.text = "0";
            _gameCursorUIService.Hide();
        }

        void OnDestroy() {
            _disposables?.Dispose();
        }

    }
}