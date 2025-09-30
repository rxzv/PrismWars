using System.Collections;
using UnityEngine;

public class Projectile : Flyweight
{
    ProjectileSettings settings => Settings as ProjectileSettings;
    
    void OnEnable()
    {
        StartCoroutine(DespawnAfterDelay(settings.DespawnDelay));
    }

    void Update()
    {
        transform.Translate(Vector3.forward * (settings.Speed * Time.deltaTime));
    }

    IEnumerator DespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        FlyweightFactory.ReturnToPool(this);
    }
}