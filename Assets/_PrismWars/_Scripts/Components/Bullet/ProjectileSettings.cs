using UnityEngine;

[CreateAssetMenu(fileName = "Flyweight", menuName = "Projectile Settings")]
public class ProjectileSettings : FlyweightSettings
{
    [SerializeField] private float _despawnDelay = 5f;
    [SerializeField] private float _speed = 10f;
    
    public float Speed {  get { return _speed; } set { _speed = value; } }
    public float DespawnDelay { get { return _despawnDelay; } set { _despawnDelay = value; } }  
    
    public override Flyweight Create()
    {
        var go = Instantiate(_prefab);
        go.SetActive(false);
        go.name = _prefab.name;
        
        var flyWeight = go.AddComponent<Projectile>();
        flyWeight.Settings = this;
        
        return flyWeight;
    }
}