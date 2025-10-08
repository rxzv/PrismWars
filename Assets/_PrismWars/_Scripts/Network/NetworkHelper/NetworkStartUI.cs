using _PrismWars._Scripts.Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace PrismWars.NetworkHelper {
    public class NetworkStartUI : MonoBehaviour {
        [SerializeField] Button _startHostButton;
        [SerializeField] Button _startClientButton;
        
        void Start() {
            _startHostButton.onClick.AddListener(StartHost);
            _startClientButton.onClick.AddListener(StartClient);
        }
        
        void StartHost() {
            Debug.Log("Starting host");
            NetworkManager.Singleton.StartHost();
            Hide();
        }

        void StartClient() {
            Debug.Log("Starting client");
            NetworkManager.Singleton.StartClient();
            Hide();
        }

        void Hide() => gameObject.SetActive(false);
    }
}