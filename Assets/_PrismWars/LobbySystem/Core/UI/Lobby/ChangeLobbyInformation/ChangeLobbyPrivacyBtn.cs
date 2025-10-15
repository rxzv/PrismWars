using System;
using UnityEngine;

public class ChangeLobbyPrivacyBtn : UIButtonEnumCycle<LobbyPrivacy>
{
    private void OnEnable()
    {
        initialValue = LobbyPrivacy.Public; 
        currentValue = initialValue;
        GameLobbyManager.Instance.IsPrivateLobby = false;
        buttonText.text = initialValue.ToString();
    }

    protected override void OnButtonClick()
    {
        base.OnButtonClick();

        if (currentValue == LobbyPrivacy.Private)
            GameLobbyManager.Instance.IsPrivateLobby = true;
        else
            GameLobbyManager.Instance.IsPrivateLobby = false;

        Debug.Log("Privacy updated: " + GameLobbyManager.Instance.IsPrivateLobby);
        
    }

    protected override void UpdateText()
    {
        if (buttonText != null)
            buttonText.text = currentValue.ToString();
    }
}
