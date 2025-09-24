using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    #region Singleton

    public static InputManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null)
            Debug.LogError("InputManager Instance already initialized");
        Instance = this;
    }

    #endregion
    
    public Action OnJumpStarted;
    
    PlayerInputActions _playerInputActions;
    
    public PlayerInputActions PlayerInputActions => _playerInputActions;
    

    private void Start()
    {
        _playerInputActions = new PlayerInputActions();
        _playerInputActions.Player.Enable();
        _playerInputActions.Player.Jump.started += _ => OnJumpStarted?.Invoke();
    }
}