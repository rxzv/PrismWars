using UnityEngine;
using UnityEngine.UI;

namespace _PrismWars._Scripts.UI {
    public class GameCursorUIService : MonoBehaviour, IService {
        [SerializeField] Image _cursorImg;
        [SerializeField] Slider _ultimateFillSlider;
        [SerializeField] VerticalLayoutGroup _bulletCountGroupView;

    }
}