using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class UIButtonEnumBase<TEnum> : MonoBehaviour where TEnum : Enum
{
    [Header("Evento al cambio testo")]
    public Action OnBtnClicked;
    
    [Header("UI Riferimenti Base")]
    public Button button;
    public TMP_Text buttonText;
    public GameObject overlay;

    protected TEnum currentValue;

    protected virtual void Start()
    {
        if (button != null)
            button.onClick.AddListener(OnButtonClick);

        UpdateText();
    }

    protected virtual void OnButtonClick()
    {
        OnBtnClicked?.Invoke();
        ShowOverlay();
    }

    protected virtual void UpdateText()
    {
        if (buttonText != null)
            buttonText.text = currentValue.ToString();
    }

    protected TEnum GetNextTextEnumValue(TEnum value)
    {
        TEnum[] values = (TEnum[])Enum.GetValues(typeof(TEnum));
        int index = Array.IndexOf(values, value);
        return values[(index + 1) % values.Length];
    }

    protected void ShowOverlay()
    {
        if (overlay != null)
            overlay.SetActive(true);
    }

    protected void HideOverlay()
    {
        if (overlay != null)
            overlay.SetActive(false);
    }

    public virtual void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
