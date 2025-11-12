using Unity.Netcode;
using UnityEngine;

public class MyNetworkManager : PersistentNetworkSingleton<MyNetworkManager>
{
    [SerializeField] string _gameSceneName = "GameScene";
    
    #region Client

    /// <summary>
    /// Starts the client and subscribes to necessary callbacks.
    /// </summary>
    public void StartClient()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager.Singleton is null. Ensure it exists in the scene.");
            return;
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        NetworkManager.Singleton.StartClient();
    }

    /// <summary>
    /// Called when a client successfully connects to the server.
    /// </summary>
    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"CLIENT CONNECTED: {clientId}");

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("Local client connection established.");
        }
        
        
    }

    /// <summary>
    /// Called when a client disconnects from the server.
    /// </summary>
    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"CLIENT DISCONNECTED: {clientId}");

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.LogWarning("Local client has been disconnected from the server.");
        }
    }

    #endregion

    #region Host

    /// <summary>
    /// Starts the host and sets up server-side callbacks.
    /// </summary>
    public void StartHost()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager.Singleton is null. Ensure it exists in the scene.");
            return;
        }

        Debug.Log("Starting host...");
        NetworkManager.Singleton.ConnectionApprovalCallback += OnConnectionApproval;
        NetworkManager.Singleton.OnClientConnectedCallback += OnHostClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnHostClientDisconnected;
        NetworkManager.Singleton.StartHost();
    }

    /// <summary>
    /// Handles client connection approval based on game logic.
    /// </summary>
    private void OnConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= GameLobbyManager.Instance.MaxPlayers)
        {
            response.Approved = false;
            response.Reason = "Game is full";
            return;
        }

        response.Approved = true;
    }

    /// <summary>
    /// Called when a new client connects to the host.
    /// </summary>
    private void OnHostClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.Count == GameLobbyManager.Instance.MaxPlayers)
        {
            Debug.Log("All players connected. Starting game...");
            SceneLoader.LoadNetwork(_gameSceneName);
        }
        Debug.Log($"HOST: Client connected with ID {clientId}");
        // Optional: send welcome message, sync game state, etc.
    }

    /// <summary>
    /// Called when a client disconnects while connected to the host.
    /// </summary>
    private void OnHostClientDisconnected(ulong clientId)
    {
        Debug.Log($"HOST: Client {clientId} disconnected");

        // Optional: handle game pause, lobby fallback, or AI substitution
        if (NetworkManager.Singleton.ConnectedClients.Count < GameLobbyManager.Instance.MaxPlayers)
        {
            Debug.LogWarning("Not enough players to continue the game.");
            // Optional: implement match reset or shutdown logic
        }
    }

    #endregion

    #region Cleanup

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnHostClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnHostClientDisconnected;
        NetworkManager.Singleton.ConnectionApprovalCallback -= OnConnectionApproval;
    }

    #endregion
}
