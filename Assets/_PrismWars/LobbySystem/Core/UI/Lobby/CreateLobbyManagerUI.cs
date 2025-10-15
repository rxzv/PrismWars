using System;
using UnityEngine;
using UnityEngine.UI;

public class CreateLobbyManagerUI : MonoBehaviour
{
    [SerializeField] private Button createLobbyBtn;

    private void OnEnable()
    {
        if (createLobbyBtn == null)
        {
            Debug.LogError("Create Lobby Button is not assigned in the inspector.");
            return;
        }

        createLobbyBtn.onClick.AddListener(LobbyCreated);
    }

    private void LobbyCreated()
    {
        GameLobbyManager.Instance.LobbyManager.CreateLobby();
    }
    
    private void OnDisable()
    {
        if (createLobbyBtn != null)
        {
            createLobbyBtn.onClick.RemoveListener(LobbyCreated);
        }
    }
}
