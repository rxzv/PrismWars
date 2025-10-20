using System.Collections.Generic;
using System.Linq;
using _PrismWars._Scripts.UI.Controller;
using _PrismWars._Scripts.UI.Model;
using _PrismWars._Scripts.UI.View;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.UI {
    public class CharacterClientSelectionManager : MonoBehaviour, IService {
        [SerializeField] List<PlayerConfig> _characterConfigs;
        [SerializeField] CharacterSelectionView _view;

        CharacterSelectionModel _model;
        CharacterSelectionController _controller;

        void Start() {
            InitializeMvc();
            ServiceLocator.Singleton.Get<CharacterServerSelectionManager>().UnavailableCharacters.OnListChanged += UnavailableCharactersOnOnListChanged;
        }

        void UnavailableCharactersOnOnListChanged(NetworkListEvent<int> changeEvent) {
            _model.SelectedCharacterUpdate(changeEvent.Value);
        }

        void RemoveUnnecessaryConfigs() {
            var playerType = NetworkManager.Singleton.LocalClientId % 2 == 0 ? PlayerType.Fire : PlayerType.Ice;
            foreach (var config in _characterConfigs.ToList()) {
                if (config.playerType != playerType) {
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
            ServiceLocator.Singleton.Get<CharacterServerSelectionManager>().UnavailableCharacters.OnListChanged -= UnavailableCharactersOnOnListChanged;
        }
    }
}