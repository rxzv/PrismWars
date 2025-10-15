using System;
using TMPro;
using UnityEngine;


public class BaseInputFieldReader : MonoBehaviour
{
    [Header("Evento al cambio testo")]
    public Action<string> OnTextChanged;
    
    [Header("InputField")]
    public TMP_InputField inputField;

    public string InputText => inputField != null ? inputField.text : string.Empty;

    protected virtual void Awake()
    {
        if (inputField == null)
            inputField = GetComponent<TMP_InputField>();
    }

    protected virtual void Start()
    {
        if (inputField != null)
            inputField.onValueChanged.AddListener(HandleTextChanged);
    }

    protected virtual void HandleTextChanged(string newText)
    {
        OnTextChanged?.Invoke(newText);
    }

    public virtual void PrintInput()
    {
        Debug.Log("Input: " + InputText);
    }
}