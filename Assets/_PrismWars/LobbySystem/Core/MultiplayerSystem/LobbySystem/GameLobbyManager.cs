using System;
using UnityEngine;

public class GameLobbyManager : PersistentSingleton<GameLobbyManager>
{
    private MyLobbyManager _lobbyManager;
    private MyRelayManager _relayManager;
    
    private string _playerName;
    private string _lobbyName = "TestLobby";
    private int _maxPlayers = 2;
    private string _gameMode = "Deathmatch";
    private string _map = "Map1";
    private bool _isPrivateLobby = false;
    private int _playerLevel = 5;

    public MyLobbyManager LobbyManager { get => _lobbyManager; private set => _lobbyManager = value; }
    public MyRelayManager RelayManager { get => _relayManager; private set => _relayManager = value; }
    
    public string PlayerName
    {
        get => _playerName;
        set => _playerName = value;
    }
    
    public string LobbyName
    {
        get => _lobbyName;
        set => _lobbyName = value;
    }

    public int MaxPlayers
    {
        get => _maxPlayers;
        set => _maxPlayers = Mathf.Clamp(value, 1, 100); 
    }

    public string GameMode
    {
        get => _gameMode;
        set => _gameMode = value;
    }

    public string Map
    {
        get => _map;
        set => _map = value;
    }
    
    public bool IsPrivateLobby
    {
        get => _isPrivateLobby;
        set => _isPrivateLobby = value;
    }
    
    public int PlayerLevel
    {
        get => _playerLevel;
        set => _playerLevel = Mathf.Max(0, value); 
    }

    protected override void Awake()
    {
        base.Awake();
        _lobbyManager = FindFirstObjectByType<MyLobbyManager>();
        if (_lobbyManager == null)
        {
            Debug.LogError("MyLobbyManager not found in the scene.");
        }
        _relayManager = FindFirstObjectByType<MyRelayManager>();
        if (_relayManager == null)
        {
            Debug.LogError("MyRelayManager not found in the scene.");
        }
        _playerName = "Player" + UnityEngine.Random.Range(1, 1000);
    }
}


public enum GameMode
{
    Deathmatch,
    FreeForAll,
    CaptureTheFlag,
    KingOfTheHill
}
public enum Map
{
    Map1,
    Map2,
    Map3,
    Map4
}
public enum MaxPlayers
{
    Two = 2,
    Three = 3,
    Four = 4,
    Six = 6,
    Eight = 8,
    Ten = 10,
    Twenty = 20,
    Fifty = 50,
    Hundred = 100
}

public enum LobbyPrivacy
{
    Private,
    Public
}