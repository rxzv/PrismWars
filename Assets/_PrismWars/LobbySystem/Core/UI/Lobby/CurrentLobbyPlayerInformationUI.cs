using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class CurrentLobbyPlayerInformationUI : MonoBehaviour
{
   [SerializeField] private TMPro.TextMeshProUGUI playerNameText;
   [SerializeField] private TMPro.TextMeshProUGUI playerLevelText;
   [SerializeField] private Button kickBtn;
   private string _playerId;

   Lobby _lobbyData;

   public void Init(Lobby lobbyData,Player player)
   {
      _lobbyData = lobbyData;
      
      _playerId = player.Id;

      string playerName = player.Data != null && player.Data.TryGetValue("PlayerName", out var value)
         ? value.Value
         : "Unknown";

      string playerLevel = _lobbyData.Data != null && _lobbyData.Data.TryGetValue("PlayerLevel", out var value1 )
         ? "Player Level : " + value1.Value 
         : "N/A";

      playerNameText.text = playerName;
      playerLevelText.text = playerLevel;

      bool isHost = lobbyData.HostId == AuthenticationService.Instance.PlayerId;
      bool isSelf = _playerId == AuthenticationService.Instance.PlayerId;

      if (kickBtn != null)
      {
         kickBtn.gameObject.SetActive(isHost && !isSelf);
         kickBtn.onClick.RemoveAllListeners();
         kickBtn.onClick.AddListener(KickPlayer);
      }
   }
   
   private void KickPlayer()
   {
      Debug.Log($"Kicking player: {playerNameText.text}");
      GameLobbyManager.Instance.LobbyManager.KickPlayer(_playerId);
      Destroy(gameObject);
   }

}
