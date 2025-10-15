using System;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class CurrentLobbyUI : MonoBehaviour
{
    public static Action OnTextLobbyCreated;
   
    [Header("UI Elements")]
    [SerializeField] private TMPro.TextMeshProUGUI lobbyNameText;
    [SerializeField] private TMPro.TextMeshProUGUI lobbyOwnerText;
    [SerializeField] private TMPro.TextMeshProUGUI lobbyPlayerCountText;
    [SerializeField] private TMPro.TextMeshProUGUI lobbyMapText;
    [SerializeField] private TMPro.TextMeshProUGUI lobbyGameModeText;
    
    private float _currentPlayerCount;
    private float _maxPlayerCount;
   
    private void OnEnable()
    {
        MyLobbyManager.OnLobbyCreated += LobbyCreated;
        MyLobbyManager.OnLobbyJoined += LobbyCreated;
        MyLobbyManager.OnJoinLobbyUpdate += LobbyCreated;
        MyLobbyManager.OnKickPlayer += KickPlayer;
        MyLobbyManager.OnLobbyLeft += KickPlayer;
    }

    private void KickPlayer()
    {
        lobbyPlayerCountText.text = $"{_currentPlayerCount - 1}/{_maxPlayerCount}";
    }

    private void LobbyCreated(Lobby obj)
    {
        lobbyNameText.text = obj.Name;

        var hostPlayer = obj.Players.Find(p => p.Id == obj.HostId);
        string hostName = hostPlayer?.Data != null && hostPlayer.Data.ContainsKey("PlayerName")
            ? hostPlayer.Data["PlayerName"].Value
            : "Unknown";
        lobbyOwnerText.text = hostName;
      
        lobbyPlayerCountText.text = $"{obj.Players.Count}/{obj.MaxPlayers}";
        _maxPlayerCount = obj.MaxPlayers;
        _currentPlayerCount = obj.Players.Count;
        lobbyMapText.text = obj.Data.ContainsKey("Map") ? obj.Data["Map"].Value : "Unknown";
        lobbyGameModeText.text = obj.Data.ContainsKey("GameMode") ? obj.Data["GameMode"].Value : "Unknown";
      
        OnTextLobbyCreated?.Invoke();
    }

    private void OnDisable()
    {
        MyLobbyManager.OnLobbyJoined -= LobbyCreated;
        MyLobbyManager.OnLobbyCreated -= LobbyCreated;
        MyLobbyManager.OnJoinLobbyUpdate -= LobbyCreated;
        MyLobbyManager.OnKickPlayer -= KickPlayer;
        MyLobbyManager.OnLobbyLeft -= KickPlayer;
    }
}
