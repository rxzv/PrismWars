using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button serverButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;

    private void Awake()
    {
        if (serverButton != null)
        {
            serverButton.onClick.AddListener(() => NetworkManager.Singleton.StartServer());
        }
        
        if (hostButton != null)
        {
            hostButton.onClick.AddListener(() => NetworkManager.Singleton.StartHost());
        }
        
        if (clientButton != null)
        {
            clientButton.onClick.AddListener(() => NetworkManager.Singleton.StartClient());
        }
    }
}
