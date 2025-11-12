using System;
using _PrismWars._Scripts;
using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Services;
using R3;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class CameraSpawnService : MonoBehaviour, IService, IDisposable, IInitializable<CinemachineCamera> {
    
    CinemachineCamera _camera;
    
    readonly CompositeDisposable _disposables = new();
    
    public void Initialize(CinemachineCamera camera) {
        _camera = camera;
        var networkPlayerSpawnService = ServiceLocator.Singleton.Get<NetworkPlayerSpawnService>();
        networkPlayerSpawnService.OnPlayerSpawned
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
