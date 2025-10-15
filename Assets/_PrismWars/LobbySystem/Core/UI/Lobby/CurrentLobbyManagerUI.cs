using System;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class CurrentLobbyManagerUI : MonoBehaviour
{
   [SerializeField] private Button leaveLobbyBtn;
   [SerializeField] private Button startGameBtn;
   [SerializeField] private TMPro.TextMeshProUGUI privateCodeText;
   

   Lobby _currentLobby;
   
   private void OnEnable()
   {
      if (leaveLobbyBtn == null)
      {
         Debug.LogError("Leave Lobby Button is not assigned in the inspector.");
         return;
      }

      startGameBtn.gameObject.SetActive(false);
      startGameBtn.onClick.AddListener(StartGame);
      leaveLobbyBtn.onClick.AddListener(LeaveLobby);
      MyLobbyManager.OnLobbyCreated += LobbyCreated;
      MyLobbyManager.OnLobbyJoined += LobbyCreated;
      MyLobbyManager.OnLobbyFull += LobbyFull;
      MyLobbyManager.OnLobbyLeft += LobbyLeft;
      MyLobbyManager.OnNewHost += NewHost;
      MyLobbyManager.OnPlayerLeft += LobbyLeft;
   }

   private void NewHost(Lobby obj)
   {
      _currentLobby = obj;
      if (_currentLobby.IsPrivate)
      {
         privateCodeText.text = _currentLobby.LobbyCode;
         privateCodeText.gameObject.SetActive(true);
      }
   }

   private void LobbyLeft()
   {
      startGameBtn.gameObject.SetActive(false);
   }

   private void LobbyFull()
   {
      startGameBtn.gameObject.SetActive(true);
   }

   private void StartGame()
   {
      GameLobbyManager.Instance.LobbyManager.StartGame();
   }

   private void LobbyCreated(Lobby obj)
   {
      _currentLobby = obj;
      if (_currentLobby.IsPrivate && _currentLobby.HostId == AuthenticationService.Instance.PlayerId)
      {
         privateCodeText.text = _currentLobby.LobbyCode;
         privateCodeText.gameObject.SetActive(true);
      }
      else
      {
         privateCodeText.gameObject.SetActive(false);
         privateCodeText.text = string.Empty;
      }
   }

   private void LeaveLobby()
   {
      GameLobbyManager.Instance.LobbyManager.LeaveLobby();
      
   }

   private void OnDisable()
   {
      if (leaveLobbyBtn != null)
      {
         leaveLobbyBtn.onClick.RemoveListener(LeaveLobby);
      }

      MyLobbyManager.OnLobbyCreated -= LobbyCreated;
      MyLobbyManager.OnLobbyJoined -= LobbyCreated;
      MyLobbyManager.OnLobbyFull -= LobbyFull;
      MyLobbyManager.OnLobbyLeft -= LobbyLeft;
      MyLobbyManager.OnNewHost -= NewHost;
      MyLobbyManager.OnPlayerLeft -= LobbyLeft;
   }
}
