using _PrismWars._Scripts.Player;
using Reflex.Core;
using UnityEngine;

namespace _PrismWars._Scripts
{
    public class GameInstaller : MonoBehaviour, IInstaller {
        // [SerializeField] PlayerConfig _config;
        
        public void InstallBindings(ContainerBuilder containerBuilder) {
            // containerBuilder.AddSingleton(_config,typeof(PlayerConfig));
        }

    }
}