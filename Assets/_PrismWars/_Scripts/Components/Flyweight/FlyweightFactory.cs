using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class FlyweightFactory : MonoBehaviour, IService
{
    [SerializeField] private bool _collectionCheck = true;
    [SerializeField] private int _defaultCapacity = 10;
    [SerializeField] private int _maxPoolSize = 100;
    
    readonly Dictionary<FlyweightType, IObjectPool<Flyweight>> _pools = new();

    public Flyweight Spawn(FlyweightSettings s) => GetPoolFor(s)?.Get();
    public void ReturnToPool(Flyweight f) => GetPoolFor(f.Settings)?.Release(f);

    IObjectPool<Flyweight> GetPoolFor(FlyweightSettings settings)
    {
        IObjectPool<Flyweight> pool;

        if (_pools.TryGetValue(settings.Type, out pool)) return pool;

        pool = new ObjectPool<Flyweight>(
            settings.Create,
            settings.OnGet,
            settings.OnRelease,
            settings.OnDestroyPoolObject,
            _collectionCheck,
            _defaultCapacity,
            _maxPoolSize);

        _pools.Add(settings.Type, pool);
        return pool;
    } 
}