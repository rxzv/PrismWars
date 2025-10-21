using UnityEngine;

namespace _PrismWars._Scripts.UI {
    public class GameUIView : MonoBehaviour {
        public void ShowView() => gameObject.SetActive(true);
        public void HideView() => gameObject.SetActive(false);

    }
}