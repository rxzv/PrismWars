using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader 
{
    /// <summary>
    /// Loads a scene 
    /// </summary>
    /// <param name="targetScene"></param>
    public static void Load(string targetScene)
    {
        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogError("Scene name is null or empty.");
            return;
        }
        
        SceneManager.LoadScene(targetScene);
    }


    /// <summary>
    /// only the server changes the scene, all clients will follow
    /// </summary>
    /// <param name="targetScene"></param>
    public static void LoadNetwork(string targetScene)
    {
        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogError("Scene name is null or empty.");
            return;
        }
        if (NetworkManager.Singleton is null)
        {
            Debug.LogError("NetworkManager is not initialized.");
            return;
        }
        NetworkManager.Singleton.SceneManager.LoadScene(targetScene, LoadSceneMode.Single);
    }

}
