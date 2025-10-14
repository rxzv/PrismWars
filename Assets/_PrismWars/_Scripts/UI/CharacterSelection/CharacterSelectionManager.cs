using System;
using System.Collections.Generic;
using _PrismWars._Scripts.UI.Controller;
using _PrismWars._Scripts.UI.Model;
using _PrismWars._Scripts.UI.View;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.UI {
    [RequireComponent(typeof(NetworkObject))]
    public class CharacterSelectionManager : NetworkBehaviour, IService {
        [SerializeField] List<PlayerConfig> _characterConfigs;
        [SerializeField] CharacterSelectionView _view;
        
        NetworkList<int> _unavailableCharacters = new NetworkList<int>(
            null,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        CharacterSelectionModel _model;
        CharacterSelectionController _controller;
        
        PlayerSpawnService _playerSpawnService;

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            Initialize();
            _unavailableCharacters.OnListChanged += UnavailableCharactersOnOnListChanged;
        }

        void UnavailableCharactersOnOnListChanged(NetworkListEvent<int> changeEvent) {
            _model.SelectedCharacterUpdate(changeEvent.Value);
        }

        public void SelectCharacter(int index) {
            SelectCharacterRpc(index);
        }

        [Rpc(SendTo.Server)]
        void SelectCharacterRpc(int index) {
            _unavailableCharacters.Add(index);
        }

        void Initialize() {
            _playerSpawnService = ServiceLocator.Current.Get<PlayerSpawnService>();
            _model = new CharacterSelectionModel(_characterConfigs);
            _controller = new CharacterSelectionController(_model, _view, _playerSpawnService);
        
            _view.InitializeCharacters(_characterConfigs);
        }

        void OnDestroy() {
            _controller?.Cleanup();
            _unavailableCharacters.OnListChanged -= UnavailableCharactersOnOnListChanged;
        }
    }
}