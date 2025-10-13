using System.Collections;
using System.Collections.Generic;
using _PrismWars._Scripts.UI.Controller;
using _PrismWars._Scripts.UI.Model;
using _PrismWars._Scripts.UI.View;
using UnityEngine;

namespace _PrismWars._Scripts.UI {
    public class CharacterSelectionManager : MonoBehaviour
    {
        [SerializeField] List<PlayerConfig> _characterConfigs;
        [SerializeField] CharacterSelectionView _view;

        CharacterSelectionModel _model;
        CharacterSelectionController _controller;
        
        PlayerSpawnService _playerSpawnService;

        void Start() {
            InitializeMVC();
        }

        void InitializeMVC() {
            _playerSpawnService = ServiceLocator.Current.Get<PlayerSpawnService>();
            _model = new CharacterSelectionModel(_characterConfigs);
            _controller = new CharacterSelectionController(_model, _view, _playerSpawnService);
        
            _view.InitializeCharacters(_characterConfigs);
        }

        void OnDestroy() {
            _controller?.Cleanup();
        }
    }
}