using System;
using System.Collections.Generic;
using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Services;
using UnityEngine;

public class ServiceLocator : MonoBehaviour {
    readonly Dictionary<string, IService> _services = new();

    public static ServiceLocator Singleton { get; private set; }

    void Awake() {
        if (Singleton != null && Singleton != this) {
            Destroy(gameObject);
            Debug.LogError("Singleton already exists, destroying singleton");
        }
        Singleton = this;
        DontDestroyOnLoad(Singleton);
    }
    public T Get<T>() where T : IService {
        string key = typeof(T).Name;
        if (!_services.ContainsKey(key)) {
            Debug.LogError($"ClientServiceLocator: {key} not registered with {GetType().Name}");
            throw new InvalidOperationException();
        }
        return (T)_services[key];
    }
    public void Register<T>(T service) where T : IService {
        string key = typeof(T).Name;
        if (_services.ContainsKey(key)) {
            Debug.LogError(
                $"ClientServiceLocator: Attempted to register service of type {key} which is already registered with the {GetType().Name}.");
            return;
        }
        _services.Add(key, service);
    }
    public void Unregister<T>() where T : IService {
        string key = typeof(T).Name;
        if (!_services.ContainsKey(key)) {
            Debug.LogError(
                $"ClientServiceLocator: Attempted to unregister service of type {key} which is not registered with the {GetType().Name}.");
            return;
        }

        _services.Remove(key);
    }
}