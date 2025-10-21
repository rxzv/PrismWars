using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.UI {
    [RequireComponent(typeof(NetworkObject))]
    public class CharacterServerSelectionManager : NetworkBehaviour, IService {
        public NetworkList<int> UnavailableCharacters { get; private set; } = new NetworkList<int>(
            null,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        [ServerRpc(RequireOwnership = false)]
        public void SelectCharacterServerRpc(int configId) {
            if (!UnavailableCharacters.Contains(configId) && configId >= 0) 
                UnavailableCharacters.Add(configId);
            else if(configId < 0)
                Debug.LogError($"Character configId {configId} is defective");
            else 
                Debug.LogError($"Character configId {configId} already exists");
            
        }
    }
}