using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
    
public class NetworkStartUI : MonoBehaviour {
    [SerializeField] Button _startHostButton;
    [SerializeField] Button _startClientButton;
    
    void Start() {
        _startHostButton.onClick.AddListener(StartHost);
        _startClientButton.onClick.AddListener(StartClient);
    }
    
    void StartHost() {
        Debug.Log("Starting host");
        PlayerPrefs.SetInt("Client", 0);
        SceneManager.LoadScene(1);
        Hide();
    }

    void StartClient() {
        Debug.Log("Starting client");
        PlayerPrefs.SetInt("Client", 1);
        SceneManager.LoadScene(1);
        Hide();
    }

    void Hide() => gameObject.SetActive(false);
}
