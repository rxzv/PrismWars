using System;
using System.Collections.Generic;
using UnityEngine;

public class ClientServiceLocator : MonoBehaviour {
    readonly Dictionary<string, IClientService> _services = new();

    public static ClientServiceLocator Singleton { get; private set; }

    void Awake() {
        if (Singleton != null && Singleton != this) {
            Destroy(gameObject);
            Debug.LogError("Singleton already exists, destroying singleton");
        }
        Singleton = this;
        DontDestroyOnLoad(Singleton);
    }
    public T Get<T>() where T : IClientService {
        string key = typeof(T).Name;
        if (!_services.ContainsKey(key)) {
            Debug.LogError($"ClientServiceLocator: {key} not registered with {GetType().Name}");
            throw new InvalidOperationException();
        }
        return (T)_services[key];
    }
    public void Register<T>(T service) where T : IClientService {
        string key = typeof(T).Name;
        if (_services.ContainsKey(key)) {
            Debug.LogError(
                $"ClientServiceLocator: Attempted to register service of type {key} which is already registered with the {GetType().Name}.");
            return;
        }
        _services.Add(key, service);
    }
    public void Unregister<T>() where T : IClientService {
        string key = typeof(T).Name;
        if (!_services.ContainsKey(key)) {
            Debug.LogError(
                $"ClientServiceLocator: Attempted to unregister service of type {key} which is not registered with the {GetType().Name}.");
            return;
        }

        _services.Remove(key);
    }
}