using System;
using System.Collections.Generic;
using System.Linq;
using _PrismWars._Scripts.UI.Controller;
using _PrismWars._Scripts.UI.Model;
using _PrismWars._Scripts.UI.View;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.UI {
    public class CharacterClientSelectionManager : MonoBehaviour, IInitializable {
        [SerializeField] List<PlayerConfig> _characterConfigs;
        [SerializeField] CharacterSelectionView _view;

        CharacterSelectionModel _model;
        CharacterSelectionController _controller;
        CharacterSelectionManager _selectionManager;
        
        public CharacterSelectionView View => _view;

        public void Initialize() {
            InitializeMvc();
            _selectionManager = ServiceLocator.Singleton.Get<CharacterSelectionManager>();
            _selectionManager.UnavailableCharacters.OnListChanged += UnavailableCharactersOnOnListChanged;
        }

        void UnavailableCharactersOnOnListChanged(NetworkListEvent<int> changeEvent) =>
            _model.SelectedCharacterUpdate(changeEvent.Value);
        

        // TODO: перенести в гейм менеджер, чтобы сервер решал какой элемент у пользователя
        void RemoveUnnecessaryConfigs() {
            var playerType = NetworkManager.Singleton.LocalClientId % 2 == 0 ? PlayerElement.Fire : PlayerElement.Ice;
            foreach (var config in _characterConfigs.ToList()) {
                if (config.playerElement != playerType) {
                    _characterConfigs?.Remove(config);
                }
            }
        }
        void InitializeMvc() {
            RemoveUnnecessaryConfigs();
            _model = new CharacterSelectionModel(_characterConfigs);
            _controller = new CharacterSelectionController(_model, _view);
        
            _view.InitializeCharacters(_characterConfigs);
        }

        void OnDestroy() {
            _controller?.Cleanup();
            _selectionManager.UnavailableCharacters.OnListChanged -= UnavailableCharactersOnOnListChanged;
        }
    }
}