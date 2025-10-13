using System;
using _PrismWars._Scripts;
using R3;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class CameraSpawnService : MonoBehaviour, IService, IDisposable, IInitializable<CinemachineCamera> {
    
    CinemachineCamera _camera;
    
    readonly CompositeDisposable _disposables = new();
    
    public void SetType(CinemachineCamera camera) {
        _camera = camera;
        ServiceLocator.Current.Get<PlayerSpawnService>().OnPlayerSpawned
            .Where(tuple => tuple.clientId == NetworkManager.Singleton.LocalClientId)
            .Subscribe(tuple => SpawnCamera(tuple.playerRef))
            .AddTo(_disposables);
    }
    
    void SpawnCamera(NetworkObjectReference playerReference) {
        Observable.EveryUpdate()
            .Select(_ => playerReference.TryGet(out NetworkObject playerObject) ? playerObject : null)
            .Where(playerObject => playerObject != null)
            .Take(1)
            .Subscribe(player => {
                var cam = Instantiate(_camera, player.transform.position, Quaternion.identity);
                cam.Follow = player.transform;
                cam.LookAt = player.transform;
            })
            .AddTo(_disposables);
    }

    public void Dispose() {
        _disposables?.Dispose();
    }

}
