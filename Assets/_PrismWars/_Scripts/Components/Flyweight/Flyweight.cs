using UnityEngine;

public class Flyweight : MonoBehaviour
{
    private FlyweightSettings _settings;
    public FlyweightSettings Settings { get { return _settings; } set { _settings = value; } }
}