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

        void Start() {
            InitializeMVC();
        }

        void InitializeMVC() {
            _model = new CharacterSelectionModel(_characterConfigs);
            _controller = new CharacterSelectionController(_model, _view);
        
            _view.InitializeCharacters(_characterConfigs);
        }

        void OnDestroy() {
            _controller?.Cleanup();
        }
    }
}