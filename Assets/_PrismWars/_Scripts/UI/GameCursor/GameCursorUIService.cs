using System;
using System.Collections.Generic;
using _PrismWars._Scripts.UI.Model;
using UnityEngine;
using UnityEngine.UI;

namespace _PrismWars._Scripts.UI {
    public class GameCursorUIService : MonoBehaviour, IService, IInitializable {
        [SerializeField] Slider _ultimateFillSlider;
        [SerializeField] VerticalLayoutGroup _bulletCountGroupView;
        [SerializeField] Image _bulletImagePrefab;
        [SerializeField] Color _bulletCooldownColor;
        [SerializeField] Color _bulletAvailableFireColor;
        [SerializeField] Color _bulletAvailableIceColor;
        
        int _maxBulletCount;
        List<Image> _bullets = new ();
        PlayerElement _playerElement;

        void Awake() {
            _ultimateFillSlider.value = 0;
        }
        
        public void Initialize() {
            var playerData = ServiceLocator.Singleton.Get<NetworkPlayerSpawnService>().PlayerData;
            _maxBulletCount = playerData.maxBulletCount;
            _playerElement = playerData.playerElement;
            SpawnBulletImage();
        }
        
        public void Show() {
            gameObject.SetActive(true);
        }
        public void Hide() {
            gameObject.SetActive(false);
        }
        
        void SpawnBulletImage() {
            for (int i = 0; i <= _maxBulletCount; i++) {
                var bulletImg = Instantiate(_bulletImagePrefab, 
                    _bulletCountGroupView.transform.position, 
                    _bulletCountGroupView.transform.rotation, 
                    _bulletCountGroupView.transform);
                bulletImg.color = _bulletCooldownColor;
                _bullets.Add(bulletImg);
            }
        }
        public void BulletCooldown(int count) {
            BulletChangeColor(count, _bulletCooldownColor);
        }
        public void BulletAvailable(int count) {
            BulletChangeColor(count, GetColorByPlayerElement());
        }

        void BulletChangeColor(int count, Color color) {
            if (count <= _maxBulletCount)
                for (int i = 0; i <= count; i++)
                    _bullets[i].color = color;
            else 
                Debug.Log("bullet count > max bullet count");
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