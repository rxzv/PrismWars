using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Services;
using _PrismWars._Scripts.Game.Player.Model;
using _PrismWars._Scripts.Game.Services.Server;
using UnityEngine;
using UnityEngine.UI;

namespace _PrismWars._Scripts.UI.Services.Mono {
    public class GameCursorUIService : MonoBehaviour, IService, IInitializable {
        [SerializeField] Slider _ultimateFillSlider;
        [SerializeField] VerticalLayoutGroup _bulletCountGroupView;
        [SerializeField] Image _bulletImagePrefab;
        [SerializeField] Color _bulletCooldownColor;
        [SerializeField] Color _bulletAvailableFireColor;
        [SerializeField] Color _bulletAvailableIceColor;
        
        int _maxBulletCount;
        Image[] _bullets;
        PlayerElement _playerElement;

        void Awake() {
            _ultimateFillSlider.value = 1;
        }
        
        public void Initialize() {
            var playerData = ServiceLocator.Singleton.Get<NetworkPlayerSpawnService>().PlayerData;
            _maxBulletCount = playerData.maxBulletCount;
            _playerElement = playerData.playerElement;
            _bullets = new Image[_maxBulletCount];
            SpawnBulletImage();
        }
        
        public void Show() {
            gameObject.SetActive(true);
        }
        public void Hide() {
            gameObject.SetActive(false);
        }
        
        void SpawnBulletImage() {
            for (int i = 0; i < _maxBulletCount; i++) {
                var bulletImg = Instantiate(_bulletImagePrefab, 
                    _bulletCountGroupView.transform.position, 
                    _bulletCountGroupView.transform.rotation, 
                    _bulletCountGroupView.transform);
                bulletImg.color = GetColorByPlayerElement();
                _bullets[i] = bulletImg;
            }
        }
        public void BulletCooldown(int index) {
            BulletChangeColor(index, _bulletCooldownColor);
        }
        public void BulletAvailable(int index) {
            BulletChangeColor(index, GetColorByPlayerElement());
        }

        void BulletChangeColor(int index, Color color) {
            if (--index <= _maxBulletCount)
                _bullets[index].color = color;
            else 
                Debug.Log("bullet index > max bullet count");
        }

        void Update() {
            transform.position = Input.mousePosition;
        }

        Color GetColorByPlayerElement() {
            return _playerElement switch {
                PlayerElement.Fire => _bulletAvailableFireColor,
                PlayerElement.Ice => _bulletAvailableIceColor,
                _ => Color.white
            };
        }
    }
}