using System;
using UnityEngine;

public class ChangeLobbyNameInputField : BaseInputFieldReader
{
    private void OnEnable()
    {
        inputField.text = GameLobbyManager.Instance.LobbyName;
        inputField.text = "TestLobby"; 
    }

    protected override void HandleTextChanged(string newText)
    {
        base.HandleTextChanged(newText);
        GameLobbyManager.Instance.LobbyName = newText;
        Debug.Log("Lobby name updated: " + GameLobbyManager.Instance.LobbyName);
    }
}