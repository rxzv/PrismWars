using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Services;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.UI.CharacterSelection {
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkCharacterSelectionManager : NetworkBehaviour, IService {
        public NetworkList<int> UnavailableCharacters { get; private set; } = new NetworkList<int>(
            null,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        [Rpc(SendTo.Server)]
        public void SelectCharacterRpc(int configId) {
            if(!IsServer) return;
            
            if (!UnavailableCharacters.Contains(configId) && configId >= 0) 
                UnavailableCharacters.Add(configId);
            else if(configId < 0)
                Debug.LogError($"Character configId {configId} is defective");
            else 
                Debug.LogError($"Character configId {configId} already exists");
            
        }
    }
}