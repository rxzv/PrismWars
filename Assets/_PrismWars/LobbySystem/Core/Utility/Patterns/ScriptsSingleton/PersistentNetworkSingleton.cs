using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PersistentNetworkSingleton<T> : NetworkBehaviour where T : Component
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
        DontDestroyOnLoad(Instance);
    }

}
