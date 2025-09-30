using UnityEngine;

[CreateAssetMenu(fileName = "Flyweight", menuName = "Flyweight Settings")]
public class FlyweightSettings : ScriptableObject
{
    [SerializeField] protected FlyweightType _type;
    [SerializeField] protected GameObject _prefab;
    
    public FlyweightType Type { get { return _type; } }

    public virtual Flyweight Create()
    {
        var go = Instantiate(_prefab);
        go.SetActive(false);
        go.name = _prefab.name;
        
        var flyWeight = go.AddComponent<Flyweight>();
        flyWeight.Settings = this;
        
        return flyWeight;
    }
    
    public virtual void OnGet(Flyweight f) => f.gameObject.SetActive(true);
    public virtual void OnRelease(Flyweight f) => f.gameObject.SetActive(false);
    public virtual void OnDestroyPoolObject(Flyweight f) => Destroy(f.gameObject);
}