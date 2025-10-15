using System;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Serialization;

public class LobbyMenuManagerUI : MonoBehaviour
{
    [SerializeField] private GameObject lobbiesListPanel;
    [SerializeField] private GameObject currentLobbyPanel;
    [SerializeField] private GameObject createLobbyPanel;
    
    
    private void OnEnable()
    {
        LobbiesListManagerUI.OnStartCreateLobby += StartCreateLobby;
        MyLobbyManager.OnLobbyJoined += LobbyJoined;
        MyLobbyManager.OnLobbyCreated += LobbyCreated;
        MyLobbyManager.OnLobbyLeft += KickPlayer;
    }

    private void KickPlayer()
    {
        TogglelobbiesListPanel(true);
        ToggleMyLobbyPanel(false);
        ToggleCreateLobbyPanel(false);
    }

    private void StartCreateLobby()
    {
        ToggleCreateLobbyPanel(true);
    }

    private void LobbyCreated(Lobby obj)
    {
        ToggleMyLobbyPanel(true);
        TogglelobbiesListPanel(false);
        ToggleCreateLobbyPanel(false);
    }

    private void LobbyJoined(Lobby obj)
    {
        ToggleMyLobbyPanel(true);
        TogglelobbiesListPanel(false);
    }

    public void TogglelobbiesListPanel(bool isActive)
    {
        if (lobbiesListPanel != null)
        {
            lobbiesListPanel.SetActive(isActive);
        }
        else
        {
            Debug.LogWarning("Lobby Panel is not assigned in the inspector.");
        }
    }
    
    public void ToggleMyLobbyPanel(bool isActive)
    {
        if (currentLobbyPanel != null)
        {
            currentLobbyPanel.SetActive(isActive);
        }
        else
        {
            Debug.LogWarning("My Lobby Panel is not assigned in the inspector.");
        }
    }
    
    public void ToggleCreateLobbyPanel(bool isActive)
    {
        if (createLobbyPanel != null)
        {
            createLobbyPanel.SetActive(isActive);
        }
        else
        {
            Debug.LogWarning("Create Lobby Panel is not assigned in the inspector.");
        }
    }

    private void OnDisable()
    {
        LobbiesListManagerUI.OnStartCreateLobby -= StartCreateLobby;
        MyLobbyManager.OnLobbyJoined -= LobbyJoined;
        MyLobbyManager.OnLobbyCreated -= LobbyCreated;
        MyLobbyManager.OnLobbyLeft -= KickPlayer;
    }
}
