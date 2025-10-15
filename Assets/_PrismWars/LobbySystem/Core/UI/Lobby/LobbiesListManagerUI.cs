
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbiesListManagerUI : MonoBehaviour
{
    public static Action OnStartCreateLobby;
    
    [SerializeField] private Button createLobbyBtn;
    [SerializeField] private Button refreshLobbyBtn;
    [SerializeField] private Button privateLobbyBtn;
    [SerializeField] private GameObject privateLobbyPanel;
    [SerializeField] private Button joinPrivateLobbyBtn;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject privateCodeErrorPanel;
    [SerializeField] private Button cancelJoinPrivateLobbyBtn;
    
    private void OnEnable()
    {
        privateCodeErrorPanel.SetActive(false);
        privateLobbyPanel.SetActive(false);
        if(createLobbyBtn == null || refreshLobbyBtn == null || privateLobbyBtn == null || inputField == null)
        {
            Debug.LogError("Buttons are not assigned in the inspector.");
            return;
        }

        createLobbyBtn.onClick.AddListener(StartCreateLobby);
        refreshLobbyBtn.onClick.AddListener(ListLobbies);
        privateLobbyBtn.onClick.AddListener(()=>privateLobbyPanel.SetActive(true));
        joinPrivateLobbyBtn.onClick.AddListener(JoinPrivateLobby);
        cancelJoinPrivateLobbyBtn.onClick.AddListener(ClearPrivateLobbyPanel);
        
        MyLobbyManager.OnPrivateCodeError += PrivateCodeError;
    }

    private void PrivateCodeError()
    {
        privateCodeErrorPanel.SetActive(true);
    }
    
    private void StartCreateLobby()
    {
        OnStartCreateLobby?.Invoke();
    }

    private void ListLobbies()
    {
        GameLobbyManager.Instance.LobbyManager.ListLobbies();
    }

    private void JoinPrivateLobby()
    {
        GameLobbyManager.Instance.LobbyManager.JoinPrivateLobbyByCode(inputField.text);
        ClearPrivateLobbyPanel();
    }
    
    private void ClearPrivateLobbyPanel()
    {
        privateLobbyPanel.SetActive(false);
        inputField.text = string.Empty;
        privateCodeErrorPanel.SetActive(false);
    }
    
    private void OnDisable()
    {
        if(createLobbyBtn != null)
        {
            createLobbyBtn.onClick.RemoveListener(StartCreateLobby);
        }
        
        if(refreshLobbyBtn != null)
        {
            refreshLobbyBtn.onClick.RemoveListener(ListLobbies);
        }
        
        if(privateLobbyBtn != null)
        {
            privateLobbyBtn.onClick.RemoveListener(JoinPrivateLobby);
        }
        if(joinPrivateLobbyBtn != null)
        {
            joinPrivateLobbyBtn.onClick.RemoveListener(JoinPrivateLobby);
        }
        if(cancelJoinPrivateLobbyBtn != null)
        {
            cancelJoinPrivateLobbyBtn.onClick.RemoveListener(() =>
            {
                privateLobbyPanel.SetActive(false);
                inputField.text = string.Empty;
                privateCodeErrorPanel.SetActive(false);
            });
        }
        MyLobbyManager.OnPrivateCodeError -= PrivateCodeError;
    }
}
