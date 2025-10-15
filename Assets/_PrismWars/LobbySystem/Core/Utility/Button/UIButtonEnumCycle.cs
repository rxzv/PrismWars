using System;
using UnityEngine;


public class UIButtonEnumCycle<TEnum> : UIButtonEnumBase<TEnum> where TEnum : Enum
{
    [Header("Valore iniziale opzionale")]
    public TEnum initialValue;
    
    /// <summary>
    /// Actual value of the button, which is the current selected value of the enum.
    /// </summary>
    public TEnum SelectedValue => currentValue;

    protected override void Start()
    {
        currentValue = initialValue;
        base.Start();
    }

    protected override void OnButtonClick()
    {
        currentValue = GetNextTextEnumValue(currentValue);
        UpdateText();
    }

    protected override void UpdateText()
    {
        int value = Convert.ToInt32(currentValue);
        buttonText.text = value.ToString();
    }
}
