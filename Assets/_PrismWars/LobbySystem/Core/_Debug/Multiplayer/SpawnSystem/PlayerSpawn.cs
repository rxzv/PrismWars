using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawn : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManagerOnOnLoadEventCompleted;
        }
    }

    private void SceneManagerOnOnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (!IsHost) return;
        StartCoroutine(SpawnPlayersWhenReady(clientsCompleted));
    }
    
    private IEnumerator SpawnPlayersWhenReady(List<ulong> clientCompleted)
    {
        yield return new WaitUntil(() => clientCompleted.Count == GameLobbyManager.Instance.MaxPlayers);
       
        foreach (var player in clientCompleted)
        {
            GameObject currentPlayer = Instantiate(playerPrefab,new Vector3(0,0,0), Quaternion.identity);
            NetworkObject networkObject = currentPlayer.GetComponent<NetworkObject>();

            if (networkObject == null)
            {
                Debug.LogError($"controllare player non ha il networkObject {networkObject}");
                break;
            }
            
            currentPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(player, true);
        }
    }
    
}
