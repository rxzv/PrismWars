using _PrismWars._Scripts;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class CameraSpawner : MonoBehaviour {
    
    [SerializeField] CinemachineCamera _camera;
    
    private void Start() {
        PlayerSpawner.Instance.OnPlayerSpawned += SpawnCamera;
    }
    
    void SpawnCamera(NetworkObjectReference playerReference, ulong clientId) {
        if (clientId != NetworkManager.Singleton.LocalClientId) return;
        
        playerReference.TryGet(out NetworkObject playerObject);
        
        Transform player = playerObject.gameObject.transform;
        var cam = Instantiate(_camera, player.position, Quaternion.identity);
        cam.Follow = player;
    }

    private void OnDestroy() {
        PlayerSpawner.Instance.OnPlayerSpawned -= SpawnCamera;
    }

}
