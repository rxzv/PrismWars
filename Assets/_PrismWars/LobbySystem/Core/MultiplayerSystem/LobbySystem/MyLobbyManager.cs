using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class MyLobbyManager : MonoBehaviour
{
    public static Action<Lobby> OnLobbyCreated;
    public static Action<Lobby> OnLobbyJoined;
    public static Action OnLobbyLeft;
    public static Action<Lobby> OnLobbyUpdated;
    public static Action OnLobbyDeleted;
    public static Action<List<Lobby>> OnLobbyFound;
    public static Action<Lobby> OnJoinLobbyUpdate;
    public static Action OnKickPlayer;
    public static Action<Lobby> OnLobbyMigrated;
    public static Action<Lobby>  OnJoinPrivateLobby;
    public static Action OnPrivateCodeError;
    public static Action OnLobbyFull;
    public static Action OnPlayerLeft;
    public static Action<Lobby> OnNewHost;
    
    private Lobby _hostLobby;
    private Lobby _joinLobby;
    private float _heartbeatTimer = 15f;
    private float _lobbyUpdatePollTimer = 2f;
    private int _previousPlayerCount = -1;

    private void Start()
    {
        InitPlayerAuthentication();
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPollForUpdates();
    }

    private async void InitPlayerAuthentication()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in successfully!" + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
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
        if (_joinLobby == null || string.IsNullOrEmpty(_joinLobby.Id)) return;

        _lobbyUpdatePollTimer -= Time.deltaTime;
        if (_lobbyUpdatePollTimer < 0)
        {
            _lobbyUpdatePollTimer = 1.1f;
            try
            {
                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(_joinLobby.Id);
                _joinLobby = lobby;

                if (_joinLobby.HostId == AuthenticationService.Instance.PlayerId)
                {
                    _hostLobby = _joinLobby;
                    
                }
                
                bool stillInLobby = _joinLobby.Players.Exists(p => p.Id == AuthenticationService.Instance.PlayerId);
                if (!stillInLobby)
                {
                    Debug.Log("you were kicked off.");
                    OnLobbyLeft?.Invoke();
                    OnJoinLobbyUpdate?.Invoke(_joinLobby);
                    _joinLobby = null;
                    return;
                }
                
                if (_joinLobby.Data["RelayCode"].Value != "0")
                {
                    if (!IsLobbyHost())
                    {
                        GameLobbyManager.Instance.RelayManager.JoinRelay(_joinLobby.Data["RelayCode"].Value);
                        OnJoinLobbyUpdate?.Invoke(_joinLobby);
                    }
                }
                
                if (_joinLobby.Players.Count != _previousPlayerCount)
                {
                    _previousPlayerCount = _joinLobby.Players.Count;
                    Debug.Log("Player count changed. Updating UI...");
                    OnPlayerLeft?.Invoke();
                    OnJoinLobbyUpdate?.Invoke(_joinLobby);
                }
                
                if (IsLobbyHost())
                {
                    if (_joinLobby.Players.Count == _joinLobby.MaxPlayers)
                    {
                        Debug.Log("Lobby is full.");
                        OnLobbyFull?.Invoke();
                    }
                }
                
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to send lobby heartbeat: {e.Message}");
                _joinLobby = null;
                _hostLobby = null;
                OnLobbyLeft?.Invoke();
            }
        }
        
    }

    public async void CreateLobby()
    {
        
        AllPlayersBanned bannedPlayers = new AllPlayersBanned { bannedPlayers = new List<BannedPlayer>() };
        try
        {
            var createLobbyOptions = new CreateLobbyOptions();
            createLobbyOptions.IsPrivate = GameLobbyManager.Instance.IsPrivateLobby;
            createLobbyOptions.Player = GetPlayer();
            createLobbyOptions.Data = new Dictionary<string, DataObject>()
            {
                {"RelayCode", new DataObject(visibility:DataObject.VisibilityOptions.Member,"0") },
                {"PlayerLevel", new DataObject(visibility:DataObject.VisibilityOptions.Public,GameLobbyManager.Instance.PlayerLevel.ToString()) },
                {"Map", new DataObject(visibility:DataObject.VisibilityOptions.Public,GameLobbyManager.Instance.Map) },
                {"GameMode", new DataObject(visibility:DataObject.VisibilityOptions.Public,GameLobbyManager.Instance.GameMode) },
                {"BannedPlayers", new DataObject(visibility:DataObject.VisibilityOptions.Public, JsonUtility.ToJson(bannedPlayers)) }
                
            };
            
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(GameLobbyManager.Instance.LobbyName, GameLobbyManager.Instance.MaxPlayers, createLobbyOptions);
            
            _hostLobby = lobby;
            _joinLobby = _hostLobby;
            
            OnLobbyCreated?.Invoke(_hostLobby);
            
            Debug.Log($"Lobby created: {lobby.Name} with ID: {lobby.Id}");
            
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            throw;
        }
    }
    
    public async void JoinLobbyById(string id)
    {
        try
        {
            JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions
            {
                Player = GetPlayer()
            };
            
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
            
            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(id,joinLobbyByIdOptions);
            _joinLobby = joinedLobby; // Update the join lobby reference
            Debug.Log($"Joined lobby: {joinedLobby.Name} with ID: {joinedLobby.Id}");
            //PrintPlayers(joinedLobby);
            OnLobbyJoined?.Invoke(joinedLobby);
            
        } catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to join lobby: {e.Message}");
        }
    }
    
    public async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            Debug.Log($"Found {queryResponse.Results.Count} lobbies before filtering.");
            
            OnLobbyFound?.Invoke(queryResponse.Results);
           
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to list lobbies: {e.Message}");
        }
    }
    
    public async void JoinPrivateLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByIdOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };
            
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode,joinLobbyByIdOptions);
            _joinLobby = joinedLobby; 
            Debug.Log($"Joined private lobby with Code: {lobbyCode}");
            OnLobbyJoined?.Invoke(joinedLobby);
        }
        catch (LobbyServiceException e)
        {
            OnPrivateCodeError?.Invoke();
            Debug.LogError($"Failed to join private lobby: {e.Message}");
        }
    }

    public async void StartGame()
    {
        if (_hostLobby == null) return;

        try
        {
            string relayCode = await GameLobbyManager.Instance.RelayManager.CreateReturnRelay();
            UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                }
            };
            _hostLobby = await LobbyService.Instance.UpdateLobbyAsync(_hostLobby.Id, updateLobbyOptions);
            _joinLobby = _hostLobby; 
           
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to start lobby : {e.Message}");
        }
    }
    
     private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, GameLobbyManager.Instance.PlayerName) }
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
            
            //PrintPlayers(_hostLobby);
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
            
            //PrintPlayers(_hostLobby);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to update lobby map: {e.Message}");
        }
    }

    public async void LeaveLobby()
    {
        if (_joinLobby == null) return;

        string playerId = AuthenticationService.Instance.PlayerId;
        
        try
        {
            if (IsLobbyHost())
            {
                Debug.Log("Host is leaving. Starting host migration.");
                MigrateLobbyHost(); 
            }
            
            await LobbyService.Instance.RemovePlayerAsync(_joinLobby.Id, playerId);
            OnLobbyLeft?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error while leaving lobby: {e.Message}");
        }
    }
    
    public async void KickPlayer(string playerId)
    {
        if (_hostLobby == null) return;

        // Parse existing banned players
        var bannedList = new AllPlayersBanned { bannedPlayers = new List<BannedPlayer>() };
        if (_hostLobby.Data.TryGetValue("BannedPlayers", out var dataObj))
        {
            try
            {
                bannedList = JsonUtility.FromJson<AllPlayersBanned>(dataObj.Value);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Errore parsing BannedPlayers: " + e.Message);
            }
        }
        
        bannedList.bannedPlayers.Add(new BannedPlayer { playerId = playerId, reason = "Kicked by host" });
        
        var updateOptions = new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject>
            {
                {"BannedPlayers", new DataObject(DataObject.VisibilityOptions.Public, JsonUtility.ToJson(bannedList)) }
            }
        };

        _hostLobby = await LobbyService.Instance.UpdateLobbyAsync(_hostLobby.Id, updateOptions);
        // Remove player
        await LobbyService.Instance.RemovePlayerAsync(_hostLobby.Id, playerId);
        Debug.Log($"Player {playerId} has been kicked and banned from the lobby.");
        OnKickPlayer?.Invoke();
    }

    private async void MigrateLobbyHost()
    {
        if (_hostLobby == null || _hostLobby.Players.Count <= 1)
        {
            Debug.Log("No players to migrate host to.");
            return;
        }

        try
        {
            _hostLobby = await LobbyService.Instance.UpdateLobbyAsync(_hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = _joinLobby.Players[1].Id
            });
            
            _joinLobby = _hostLobby; 
            
            Debug.Log($"Host migrated to: {_joinLobby.Players[1].Id}");
            OnLobbyMigrated?.Invoke(_joinLobby);
            OnJoinLobbyUpdate?.Invoke(_joinLobby);
        }
        catch (Exception e)
        {
            Debug.LogError($"Host migration failed: {e.Message}");
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
}

[Serializable]
public struct BannedPlayer
{
    public string playerId;
    public string reason;
}

[Serializable]
public struct AllPlayersBanned
{
    public List<BannedPlayer> bannedPlayers;
}

