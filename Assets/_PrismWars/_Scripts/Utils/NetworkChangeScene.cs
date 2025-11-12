using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Services;
using UnityEngine;

namespace _PrismWars._Scripts.Utils {
    public class NetworkChangeScene : MonoBehaviour, IService{
        public void ChangeScene(string sceneName) {
            SceneLoader.LoadNetwork(sceneName);
        }
    }
}