using _PrismWars._Scripts.UI.Controller;
using _PrismWars._Scripts.UI.Model;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.UI {
    [RequireComponent(typeof(NetworkObject))]
    public class CharacterServerSelectionManager : NetworkBehaviour, IService {
        public NetworkList<int> UnavailableCharacters = new NetworkList<int>(
            null,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
        
        
        [Rpc(SendTo.ClientsAndHost)]

        [ServerRpc(RequireOwnership = false)]
        public void SelectCharacterServerRpc(int index) {
            UnavailableCharacters.Add(index);
        }
    }
}