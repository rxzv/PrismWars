using System;
using UnityEngine;

public class ChangePlayerNumBtn : UIButtonEnumCycle<MaxPlayers>
{
    private void OnEnable()
    {
        initialValue = MaxPlayers.Two; 
        currentValue = initialValue;
        GameLobbyManager.Instance.MaxPlayers = (int)initialValue;
        buttonText.text = GameLobbyManager.Instance.MaxPlayers.ToString();
    }

    protected override void OnButtonClick()
    {
        base.OnButtonClick();
        GameLobbyManager.Instance.MaxPlayers = (int)SelectedValue;
        Debug.Log("Max players updated: " + GameLobbyManager.Instance.MaxPlayers);
        
    }

}
