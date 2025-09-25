using System;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystem : MonoBehaviour
{
    #region Singleton

    public static InputSystem Instance { get; private set; }

    void Awake()
    {
        if (Instance != null)
            Debug.LogError("InputManager Instance already initialized");
        Instance = this;
    }

    #endregion
    public Observable<Vector2> MoveInput { get; private set; }
    public readonly ReactiveCommand JumpCommand = new();
    
    PlayerInputActions _inputActions;
    readonly CompositeDisposable _disposables = new();

    private void Start()
    {
        _inputActions = new PlayerInputActions();
        _inputActions.Player.Enable();

        MoveInput = Observable.EveryUpdate()
            .Select(_ => _inputActions.Player.Move.ReadValue<Vector2>());
        
        Observable.FromEvent<InputAction.CallbackContext>(
                h => _inputActions.Player.Jump.performed += h,
                h => _inputActions.Player.Jump.performed -= h)
            .Subscribe(_ => JumpCommand.Execute(Unit.Default))
            .AddTo(_disposables);
    }

    public void Dispose() {
        _inputActions?.Dispose();
        _disposables?.Dispose();
    }
}