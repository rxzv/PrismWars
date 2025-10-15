using Unity.Netcode;
using UnityEngine;

public class NetworkSingleton<T> : NetworkBehaviour where T : Component
{
    public static T Instance;

    protected virtual void Awake()
    {
        if (Instance == null)
        {
            if (!TryGetComponent<T>(out Instance))
            {
                Instance = gameObject.AddComponent<T>();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

}
