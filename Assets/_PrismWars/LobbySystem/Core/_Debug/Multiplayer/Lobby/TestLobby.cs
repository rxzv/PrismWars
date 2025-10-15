using System;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class TestLobby : MonoBehaviour
{
    
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button listLobbiesButton;
    [SerializeField] private Button joinLobbiesButton;
    [SerializeField] private Button updateLobbiesButton;
    [SerializeField] private Button startGameButton;
    
    private Lobby _hostLobby;
    private Lobby _joinLobby;
    private float _heartbeatTimer;
    private float _lobbyUpdatePollTimer;
    private string _playerName;

    private void Awake()
    {
        if (createLobbyButton != null)
        {
            createLobbyButton.onClick.AddListener(CreateLobby);
        }
        if (listLobbiesButton != null)
        {
            listLobbiesButton.onClick.AddListener(ListLobbies);
        }
        if (joinLobbiesButton != null)
        {
            joinLobbiesButton.onClick.AddListener(JoinLobby);
        }
        if (updateLobbiesButton != null)
        {
            updateLobbiesButton.onClick.AddListener(() => UpdateLobbyGameMode("CaptureTheFlag"));
        }
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(StartGame);
        }
        _playerName = "Lollo" + UnityEngine.Random.Range(1, 10000);
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in successfully!" + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
    
    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPollForUpdates();
    }
    
    private async void HandleLobbyHeartbeat()
    {
       if(_hostLobby != null)
       {
           _heartbeatTimer -= Time.deltaTime;
           if (_heartbeatTimer < 0) 
           {
               _heartbeatTimer = 15f;
                try
                {
                     await LobbyService.Instance.SendHeartbeatPingAsync(_hostLobby.Id);
                     Debug.Log($"Lobby heartbeat sent. Lobby ID: {_hostLobby.Id}");
                }
                catch (Exception e)
                {
                     Debug.LogError($"Failed to send lobby heartbeat: {e.Message}");
                }
           }
       }
    }
    
    private async void HandleLobbyPollForUpdates()
    {
        if(_joinLobby != null)
        {
            _lobbyUpdatePollTimer -= Time.deltaTime;
            if (_lobbyUpdatePollTimer < 0) 
            {
                _lobbyUpdatePollTimer = 2f;
                try
                {
                    Lobby lobby = await LobbyService.Instance.GetLobbyAsync(_joinLobby.Id);
                    _joinLobby = lobby;

                    if (_joinLobby.Data["RelayCode"].Value != "0")
                    {
                        if (!IsLobbyHost())
                        {
                            TestRelay.Instance.JoinRelay(_joinLobby.Data["RelayCode"].Value);
                        }

                        _joinLobby = null;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to send lobby heartbeat: {e.Message}");
                }
            }
        }
    }

    private async void CreateLobby()
    {
        try
        {
            string lobbyName = "TestLobby";
            int maxPlayers = 4;
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                //IsPrivate = true; // Uncomment this line to make the lobby private small code to identify the lobby as private
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    {"GameMode", new DataObject(DataObject.VisibilityOptions.Public,"DeathMatch",DataObject.IndexOptions.S1) },
                    {"Map", new DataObject(DataObject.VisibilityOptions.Public,"Desert",DataObject.IndexOptions.S2) },
                    {"RelayCode", new DataObject(DataObject.VisibilityOptions.Member,"0",DataObject.IndexOptions.S3) }
                }
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            
            _hostLobby = lobby;
            _joinLobby = _hostLobby;
            
            Debug.Log($"Lobby created: {lobby.Name} with ID: {lobby.Id}");
            
            PrintPlayers(_hostLobby);
            
        } catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to create lobby: {e.Message}");
        }
    }

    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots,"0",QueryFilter.OpOptions.GT),
                    //new QueryFilter(QueryFilter.FieldOptions.S1,"DeathMatch",QueryFilter.OpOptions.EQ)
                    
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false,QueryOrder.FieldOptions.Created)
                }
            };
            
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            Debug.Log($"Found {queryResponse.Results.Count} lobbies:");
        
            foreach (var lobby in queryResponse.Results)
            {
                Debug.Log($"Lobby Name: {lobby.Name}, ID: {lobby.Id}, Players: {lobby.MaxPlayers}");
            }
        } catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to list lobbies: {e.Message}");
        }
    }

    private async void JoinLobby()
    {
        try
        {
            JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions
            {
                Player = GetPlayer()
            };
            
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
            
            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(queryResponse.Results[0].Id,joinLobbyByIdOptions);
            _joinLobby = joinedLobby; // Update the join lobby reference
            Debug.Log($"Joined lobby: {queryResponse.Results[0].Name} with ID: {queryResponse.Results[0].Id}");
            PrintPlayers(joinedLobby);
            
        } catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to join lobby: {e.Message}");
        }
    }
    
    private async void JoinPrivateLobbyByCode(string lobbyCode)
    {
        try
        {
            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
            _hostLobby = joinedLobby; // Update the host lobby reference
            Debug.Log($"Joined private lobby with Code: {lobbyCode}");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to join private lobby: {e.Message}");
        }
    }
    
    private async void QuickJoinLobby()
    {
        try
        {
            await LobbyService.Instance.QuickJoinLobbyAsync();
            Debug.Log("Quick joined a lobby successfully.");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to join private lobby: {e.Message}");
        }
    }


    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _playerName) }
            }
        };
    }
    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log($"Players in Lobby: {lobby.Name} + Lobby Data: {lobby.Data["GameMode"].Value} + Lobby Map : {lobby.Data["Map"].Value}");
        foreach (Player player in lobby.Players)
        {
            Debug.Log($"Player ID: {player.Id}, Name: {player.Data["PlayerName"].Value}");
        }
    }
    
    private async void UpdateLobbyGameMode(string gameMode)
    {
        if (_hostLobby == null) return;

        try
        {
            UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode, DataObject.IndexOptions.S1) }
                }
            };
            _hostLobby = await LobbyService.Instance.UpdateLobbyAsync(_hostLobby.Id, updateLobbyOptions);
            
            _joinLobby = _hostLobby; // Update the join lobby reference
            
            PrintPlayers(_hostLobby);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to update lobby game mode: {e.Message}");
        }
    }
    
    private async void UpdateLobbyMap(string map)
    {
        if (_hostLobby == null) return;

        try
        {
            UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "Map", new DataObject(DataObject.VisibilityOptions.Public, map, DataObject.IndexOptions.S2) }
                }
            };
            _hostLobby = await LobbyService.Instance.UpdateLobbyAsync(_hostLobby.Id, updateLobbyOptions);
            
            _joinLobby = _hostLobby; // Update the join lobby reference
            
            PrintPlayers(_hostLobby);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to update lobby map: {e.Message}");
        }
    }
    
    private async void UpdatePlayerName(string playerName)
    {
        if (_hostLobby == null) return;

        try
        {
            _playerName = playerName;
            UpdatePlayerOptions updatePlayerOptions = new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
                }
            };
            await LobbyService.Instance.UpdatePlayerAsync(_joinLobby.Id, AuthenticationService.Instance.PlayerId, updatePlayerOptions);
            
            PrintPlayers(_hostLobby);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to update player name: {e.Message}");
        }
    }

    private async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(_joinLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to update player name: {e.Message}");
        }
    }
    
    private void KickPlayer(string playerId)
    {
        if (_hostLobby == null) return;

        try
        {
            LobbyService.Instance.RemovePlayerAsync(_hostLobby.Id, playerId);
            Debug.Log($"Player {playerId} has been kicked from the lobby.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to kick player: {e.Message}");
        }
    }

    private async void MigrateLobbyHost()
    {
        if (_hostLobby == null) return;

        try
        {
            _hostLobby = await LobbyService.Instance.UpdateLobbyAsync(_hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = _joinLobby.Players[1].Id
            });
            
            _joinLobby = _hostLobby; 
            
            PrintPlayers(_hostLobby);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to update lobby map: {e.Message}");
        }
    }
    
    private async void DeleteLobby()
    {
        if (_hostLobby == null) return;

        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(_hostLobby.Id);
            _hostLobby = null;
            _joinLobby = null;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to delete lobby: {e.Message}");
        }
    }
    
    public bool IsLobbyHost() {
        return _joinLobby != null && _joinLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private async void StartGame()
    {
        if (IsLobbyHost())
        {
            try
            {
                Debug.Log("Starting game...");

                string relayCode = await TestRelay.Instance.CreateReturnRelay();
                
                if (relayCode != null)
                {
                    Lobby lobby = await LobbyService.Instance.UpdateLobbyAsync(_hostLobby.Id, new UpdateLobbyOptions
                    {
                        Data = new Dictionary<string, DataObject>
                        {
                            { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                        }
                    });
                    
                    _joinLobby = lobby;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete lobby: {e.Message}");
            }
        }
    }
}

