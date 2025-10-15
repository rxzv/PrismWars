
using System;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SingleLobbyDataUI : MonoBehaviour
{
    
    [Header("UI Elements")]
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private TMPro.TextMeshProUGUI lobbyNameText;
    [SerializeField] private TMPro.TextMeshProUGUI playerCountText;
    [SerializeField] private TMPro.TextMeshProUGUI lobbyGameModeText;
    [SerializeField] private GameObject bannedPanel;
    [SerializeField] private Button joinLobbyButton;
    
    private Lobby _lobbyData;
    private bool _isBanned;

    private void OnEnable()
    {
        MyLobbyManager.OnJoinLobbyUpdate += JoinLobbyUpdate;
        
        if (joinLobbyButton != null)
        {
            joinLobbyButton.onClick.AddListener(JoinLobby);
        }
    }

    private void JoinLobbyUpdate(Lobby obj)
    {
        UpdateLobbyData(obj);
    }

    private void JoinLobby()
    {
        if (_isBanned)
        {
            Debug.LogWarning("Non puoi unirti: sei stato bannato.");
            return;
        }

        GameLobbyManager.Instance.LobbyManager.JoinLobbyById(_lobbyData.Id);
    }

    public void InitLobbyData(Lobby lobby,bool isBanned)
    {
        _lobbyData = lobby;
        _isBanned = isBanned;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_lobbyData == null) return;

        lobbyNameText.text = _lobbyData.Name;
        lobbyGameModeText.text = _lobbyData.Data.ContainsKey("GameMode") 
            ? _lobbyData.Data["GameMode"].Value 
            : "Unknown";
        playerCountText.text = $"{_lobbyData.Players.Count}/{_lobbyData.MaxPlayers}";

        if (bannedPanel != null)
        {
            bannedPanel.SetActive(_isBanned);
        }
    }
    
    private void UpdateLobbyData(Lobby lobby)
    {
        _lobbyData = lobby;
        UpdateUI();
    }

    private void OnDisable()
    {
        MyLobbyManager.OnJoinLobbyUpdate -= JoinLobbyUpdate;
    }
}
