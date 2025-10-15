using System;
using UnityEngine;

public class ChangeLobbyMapBtn : UIButtonEnumCycle<Map>
{
    private void OnEnable()
    {
        initialValue = Map.Map1;
        currentValue = initialValue;
        GameLobbyManager.Instance.Map = initialValue.ToString();
        buttonText.text = initialValue.ToString();
    }

    protected override void OnButtonClick()
    {
        base.OnButtonClick(); 
        GameLobbyManager.Instance.Map = SelectedValue.ToString();
    }

    protected override void UpdateText()
    {
        if (buttonText != null)
            buttonText.text = currentValue.ToString();
    }
}
