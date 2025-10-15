using System;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class CurrentLobbyContainerUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject currentLobbyPlayerInformationPrefab; 
    public GameObject contentPanel;
    
    
    private void OnEnable()
    {
        MyLobbyManager.OnLobbyCreated += LobbyJoined;
        MyLobbyManager.OnLobbyJoined += LobbyJoined;
        MyLobbyManager.OnJoinLobbyUpdate += LobbyJoined;
 
    }

    private void LobbyJoined(Lobby obj)
    {
        ClearPlayerList();
        obj.Players.Sort((a, b) => a.Id == obj.HostId ? -1 : (b.Id == obj.HostId ? 1 : 0));
        foreach (var player in obj.Players)
        {
            var playerUI = Instantiate(currentLobbyPlayerInformationPrefab, contentPanel.transform);
            if (playerUI.TryGetComponent<CurrentLobbyPlayerInformationUI>(out var infoUIComponent))
            {
                infoUIComponent.Init(obj, player);
            }
        }
    }

    private void ClearPlayerList()
    {
        foreach (Transform child in contentPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void OnDisable()
    {
        MyLobbyManager.OnLobbyJoined -= LobbyJoined;
        MyLobbyManager.OnLobbyCreated -= LobbyJoined;
        MyLobbyManager.OnJoinLobbyUpdate -= LobbyJoined;
    }
}
