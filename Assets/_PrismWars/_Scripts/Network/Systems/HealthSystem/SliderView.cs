using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.SimpleReactiveExample.Scripts
{
    public class SliderView : MonoBehaviour
    {
        [SerializeField] Slider _slider;

        ReadOnlyReactiveProperty<float> _current;
        ReadOnlyReactiveProperty<float> _max;
        
        CompositeDisposable _disposables = new();

        public void Initialize(ReadOnlyReactiveProperty<float> current, ReadOnlyReactiveProperty<float> max)
        {
            _current = current;
            _max = max;

            _current
                .Subscribe(b => OnCurrentChanged(b))
                .AddTo(_disposables);

            UpdateValue(_current.CurrentValue, _max.CurrentValue);
        }


        void OnDestroy()
        {
            _disposables?.Dispose();
        }

        void OnCurrentChanged(float newValue) => UpdateValue(newValue, _max.CurrentValue);

        void UpdateValue(float currentValue, float maxValue) => _slider.value = currentValue / maxValue;
    }
}