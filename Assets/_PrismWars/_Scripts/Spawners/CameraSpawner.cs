using _PrismWars._Scripts;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class CameraSpawner : MonoBehaviour {
    
    [SerializeField] CinemachineCamera _camera;
    
    private void Start() {
        PlayerSpawner.Instance.OnPlayerSpawned += SpawnCamera;
    }
    
    void SpawnCamera(NetworkObjectReference player, ulong clientId) {
        if (clientId != NetworkManager.Singleton.LocalClientId) return;
        player.TryGet(out NetworkObject playerObject);
        var cam = Instantiate(_camera, playerObject.gameObject.transform.position, Quaternion.identity);
        cam.Follow = playerObject.gameObject.transform;
    }
}
