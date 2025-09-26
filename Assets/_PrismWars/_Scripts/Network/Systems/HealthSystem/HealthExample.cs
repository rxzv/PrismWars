using _PrismWars._Scripts.Player;
using UnityEngine;

namespace Assets.SimpleReactiveExample.Scripts
{
    public class HealthExample : MonoBehaviour
    {

        PlayerController pl;

        void Update() {
            pl = FindAnyObjectByType<PlayerController>();
            
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                pl.Health.Add(10);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
                pl.Health.Reduce(10);
        }
    }
}