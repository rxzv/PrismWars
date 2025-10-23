using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class ServerServiceLocator : NetworkBehaviour {
    readonly Dictionary<string, IServerService> _services = new();

    public static ServerServiceLocator Singleton { get; private set; }

    void Awake() {
        if (Singleton != null && Singleton != this) {
            Destroy(gameObject);
            Debug.LogError("ServerServiceLocator: Singleton already exists, destroying singleton");
        }
        Singleton = this;
        DontDestroyOnLoad(Singleton);
    }
    public T Get<T>() where T : IServerService {
        string key = typeof(T).Name;
        if (!_services.ContainsKey(key)) {
            Debug.LogError($"ServerServiceLocator: {key} not registered with {GetType().Name}");
            throw new InvalidOperationException();
        }
        return (T)_services[key];
    }
    public void Register<T>(T service) where T : IServerService {
        string key = typeof(T).Name;
        if (_services.ContainsKey(key)) {
            Debug.LogError(
                $"ServerServiceLocator: Attempted to register service of type {key} which is already registered with the {GetType().Name}.");
            return;
        }
        _services.Add(key, service);
    }
    public void Unregister<T>() where T : IServerService {
        string key = typeof(T).Name;
        if (!_services.ContainsKey(key)) {
            Debug.LogError(
                $"ServerServiceLocator: Attempted to unregister service of type {key} which is not registered with the {GetType().Name}.");
            return;
        }

        _services.Remove(key);
    }
}