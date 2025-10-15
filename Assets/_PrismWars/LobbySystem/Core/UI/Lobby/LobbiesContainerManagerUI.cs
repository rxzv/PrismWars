using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbiesListContainerUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject lobbyItemPrefab; 
    public GameObject contentPanel;

    private void OnEnable()
    {
        MyLobbyManager.OnLobbyFound += LobbyFound;
        MyLobbyManager.OnLobbyCreated += ClearLobbyList;
        MyLobbyManager.OnLobbyJoined += ClearLobbyList;
        MyLobbyManager.OnLobbyLeft += LobbyLeft;
        MyLobbyManager.OnKickPlayer += LobbyLeft;
    }

    private void LobbyLeft()
    {
        foreach (Transform child in contentPanel.transform)
        {
            Destroy(child.gameObject);
        }

        GameLobbyManager.Instance.LobbyManager.ListLobbies();
    }

    private void ClearLobbyList(Lobby obj)
    {
        foreach (Transform child in contentPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void LobbyFound(List<Lobby> obj)
    {
        DestroyLobbyList();

        string myId = Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;

        foreach (var lobby in obj)
        {
            bool isBanned = false;

            if (lobby.Data.TryGetValue("BannedPlayers", out var bannedData))
            {
                try
                {
                    var bannedList = JsonUtility.FromJson<AllPlayersBanned>(bannedData.Value);
                    isBanned = bannedList.bannedPlayers.Any(p => p.playerId == myId);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Errore nel parsing della lista dei bannati: " + e.Message);
                }
            }

            GameObject lobbyItem = Instantiate(lobbyItemPrefab, contentPanel.transform);
            SingleLobbyDataUI lobbyItemUI = lobbyItem.GetComponent<SingleLobbyDataUI>();
            if (lobbyItemUI != null)
            {
                lobbyItemUI.InitLobbyData(lobby, isBanned);
            }
            else
            {
                Debug.LogError("LobbyItemUI component not found on the instantiated prefab.");
            }
        }
    }
    
    private void DestroyLobbyList()
    {
        // Clean up any instantiated lobby items
        foreach (Transform child in contentPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void OnDisable()
    {
        MyLobbyManager.OnLobbyCreated -= ClearLobbyList;
        MyLobbyManager.OnLobbyJoined -= ClearLobbyList;
        MyLobbyManager.OnLobbyFound -= LobbyFound;
        MyLobbyManager.OnLobbyLeft -= LobbyLeft;
        MyLobbyManager.OnKickPlayer -= LobbyLeft;
        DestroyLobbyList();
    }

    private void OnDestroy()
    {
        
        DestroyLobbyList();
    }
}
