using System;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputService : MonoBehaviour, IService, IInitializable, IDisposable
{
    public Observable<Vector2> MoveInput { get; private set; }
    public readonly ReactiveCommand JumpCommand = new();
    
    PlayerInputActions _inputActions;
    readonly CompositeDisposable _disposables = new();

    public void Initialize()
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