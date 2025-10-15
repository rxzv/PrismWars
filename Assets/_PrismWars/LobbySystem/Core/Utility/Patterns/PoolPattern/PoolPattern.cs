using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PoolPattern : Singleton<PoolPattern>
{

    
    #region Online PoolPattern
    public SerializableDictionary<ulong, List<GameObject>> objectAvailableNew = new SerializableDictionary<ulong, List<GameObject>>();
    public SerializableDictionary<ulong, List<GameObject>> objectInUseNew = new SerializableDictionary<ulong, List<GameObject>>();

    public List<NetworkObject> InitialMultiplayerGeneration(int amount, GameObject objectToSpawn, ulong owner)
    {
        var spawnedNetworkObjects = new List<NetworkObject>();

        if (objectToSpawn == null)
        {
            Debug.LogWarning("Prefab in pool empty! No Preload happening. Please check references.");
            return spawnedNetworkObjects;
        }

        if (!objectAvailableNew.ContainsKey(owner))
        {
            objectAvailableNew[owner] = new List<GameObject>();

            objectInUseNew[owner] = new List<GameObject>();
        }

        for (int i = 0; i < amount; i++)
        {
            GameObject obj = Instantiate(objectToSpawn, transform.position, Quaternion.identity);

            if (obj.TryGetComponent(out NetworkObject netObj))
            {
                netObj.SpawnWithOwnership(owner,true);
                spawnedNetworkObjects.Add(netObj);

                obj.SetActive(false);

                Rename(obj, $"Owner {owner}, Amount {i}");

                objectAvailableNew[owner].Add(obj);

            }
            else
            {
                Debug.LogError("Oggetto spawnato nel pool senza NetworkObject.");
            }
        }

        return spawnedNetworkObjects;
    }

    /// <summary>
    /// Get a Object from the availables Online game
    /// </summary>
    /// <returns></returns>
    public GameObject GetObjectOnline(GameObject objectToSpawn, ulong clientID)
    {
        if (objectAvailableNew.TryGetValue(clientID, out var availableObjects) && availableObjects.Count > 0)
        {
            GameObject obj = availableObjects[0];
            availableObjects.RemoveAt(0);

            objectInUseNew[clientID].Add(obj);

            //m_ObjectInUse.Add(obj);
            return obj;
        }
        else
        {
            if (objectToSpawn == null) return null;
            GameObject newObj = Instantiate(objectToSpawn, gameObject.transform);
            //m_ObjectInUse.Add(newObj);
            objectInUseNew[clientID].Add(newObj);

            if (newObj.TryGetComponent(out NetworkObject netObj))
            {
                netObj.SpawnWithOwnership(clientID);
            }

            return newObj;
        }
    }

    /// <summary>
    /// Release the Object out of scene for Online game
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="clientId"></param>
    public void DisableMultiplayerBullet(GameObject obj, ulong clientId)
    {
        if (!objectInUseNew[clientId].Contains(obj))
        {
            Debug.LogWarning($"L'oggetto {obj.name} non � presente nella lista ObjectInUseNew per il client {clientId}.");
            return;
        }

        bool removed = objectInUseNew[clientId].Remove(obj);
        if (!removed)
        {
            Debug.LogError($"Errore nella rimozione di {obj.name} da ObjectInUseNew per il client {clientId}.");
            return;
        }


        if (objectAvailableNew[clientId].Contains(obj))
        {
            Debug.LogWarning($"L'oggetto {obj.name} � gi� presente nella lista ObjectAvailableNew per il client {clientId}.");
            return;
        }

        objectAvailableNew[clientId].Add(obj);
        obj.transform.position = gameObject.transform.position;
        obj.transform.rotation = Quaternion.identity;

        //Debug.Log($"Oggetto {obj.name} correttamente disattivato e spostato nella lista ObjectAvailableNew per il client {clientId}.");
    }
    #endregion

    public void Rename(GameObject instance,string name)
    {
        instance.name = name;
    }


    #region Offline PoolPattern

    protected List<GameObject> ObjectAvailable = new List<GameObject>();
    protected List<GameObject> m_ObjectInUse = new List<GameObject>();

    /// <summary>
    /// Create an amount of Objects in offline game
    /// </summary>
    /// <param name="amount"></param>
    public void InitialGeneration(int amount,GameObject objectToSpawn)
    {
        for (int i = 0; i < amount; i++)
        {
            ObjectAvailable.Add(Instantiate(objectToSpawn, gameObject.transform.position,Quaternion.identity,gameObject.transform));
            ObjectAvailable[i].SetActive(false);
        }
    }

   

    /// <summary>
    /// Get a Object from the availables offline game
    /// </summary>
    /// <param name="objectToSpawn"></param>
    /// <returns></returns>
    public GameObject GetObject(GameObject objectToSpawn)
    {
        if (ObjectAvailable.Count != 0)
        {
            GameObject go = ObjectAvailable[0];
            m_ObjectInUse.Add(go);
            ObjectAvailable.RemoveAt(0);
            go.SetActive(true);
            return go;
        }
        else
        {
            GameObject go = Instantiate(objectToSpawn, gameObject.transform);
            m_ObjectInUse.Add(go);
            return go;
        }
    }


    /// <summary>
    /// Release the Object out of scene for offline game
    /// </summary>
    /// <param name="obj"></param>
    public void ReleaseObject(GameObject obj)
    {
        obj.SetActive(false);
        ObjectAvailable.Add(obj);
        m_ObjectInUse.Remove(obj);
        obj.transform.position = gameObject.transform.position;
        obj.transform.rotation = Quaternion.identity;
    }

    #endregion

}
