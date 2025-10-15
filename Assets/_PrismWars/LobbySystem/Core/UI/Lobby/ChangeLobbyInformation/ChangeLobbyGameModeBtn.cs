using System;
using UnityEngine;

public class ChangeLobbyGameModeBtn : UIButtonEnumCycle<GameMode>
{

    private void OnEnable()
    {
        initialValue = GameMode.Deathmatch;
        currentValue = initialValue;
        GameLobbyManager.Instance.GameMode = initialValue.ToString();
        buttonText.text = initialValue.ToString();
    }

    protected override void OnButtonClick()
    {
        base.OnButtonClick(); 
        GameLobbyManager.Instance.GameMode = SelectedValue.ToString(); 
    }

    protected override void UpdateText()
    {
        if (buttonText != null)
            buttonText.text = currentValue.ToString();
    }
    
}
