using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class FlyweightFactory : MonoBehaviour
{
    [SerializeField] private bool _collectionCheck = true;
    [SerializeField] private int _defaultCapacity = 10;
    [SerializeField] private int _maxPoolSize = 100;
    
    static FlyweightFactory _instance;
    readonly Dictionary<FlyweightType, IObjectPool<Flyweight>> _pools = new();

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static Flyweight Spawn(FlyweightSettings s) => _instance.GetPoolFor(s)?.Get();
    public static void ReturnToPool(Flyweight f) => _instance.GetPoolFor(f.Settings)?.Release(f);

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