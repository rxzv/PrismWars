using R3;
using UnityEngine;

public class Health
{
    private ReactiveProperty<float> _max;
    private ReactiveProperty<float> _current;

    public Health(float current, float max)
    {
        _current = new ReactiveProperty<float>(current);
        _max = new ReactiveProperty<float>(max);
    }

    public ReadOnlyReactiveProperty<float> Max => _max;
    public ReadOnlyReactiveProperty<float> Current => _current;

    public void Reduce(float value)
    {
        if (value < 0)
        {
            Debug.LogError(nameof(value));
            return;
        }

        _current.Value = Mathf.Clamp(Current.CurrentValue - value, 0, Max.CurrentValue);
    }

    public void Add(float value)
    {
        if (value < 0)
        {
            Debug.LogError(nameof(value));
            return;
        }

        _current.Value = Mathf.Clamp(Current.CurrentValue + value, 0, Max.CurrentValue);
    }
}