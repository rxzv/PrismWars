using Unity.Netcode;

namespace _PrismWars._Scripts.Game.Services {
    public class PlayerRespawnService : NetworkBehaviour, IService {

        public void PlayerDied(NetworkObjectReference player) {
            player.TryGet(out NetworkObject networkPlayer);
            if (networkPlayer != null) {
                PlayerDiedServerRpc(networkPlayer);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void PlayerDiedServerRpc(NetworkObjectReference player) {
            player.TryGet(out NetworkObject networkPlayer);
            if (networkPlayer != null) {
                networkPlayer.gameObject.SetActive(false);
            }
        }

    }
}