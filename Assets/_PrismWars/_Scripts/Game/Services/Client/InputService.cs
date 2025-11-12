using System;
using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Services;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputService : MonoBehaviour, IService, IInitializable, IDisposable
{
    public Observable<Vector2> MoveInput { get; private set; }
    public readonly ReactiveCommand JumpCommand = new();
    public readonly ReactiveCommand AttackMelee = new();
    public readonly ReactiveCommand AttackRange = new();
    public readonly ReactiveCommand Interact = new();
    
    PlayerInputActions _inputActions;
    readonly CompositeDisposable _disposables = new();

    public void InputActionEnable() {
        _inputActions.Player.Enable();
    }
    public void InputActionDisable() {
        _inputActions.Player.Disable();
    }

    public void Initialize() {
        _inputActions = new PlayerInputActions();
        InputActionEnable();

        MoveInput = Observable.EveryUpdate()
            .Select(_ => _inputActions.Player.Move.ReadValue<Vector2>());
        
        Observable.FromEvent<InputAction.CallbackContext>(
                h => _inputActions.Player.Jump.started += h,
                h => _inputActions.Player.Jump.started -= h)
            .Subscribe(_ => JumpCommand.Execute(Unit.Default))
            .AddTo(_disposables);
        Observable.FromEvent<InputAction.CallbackContext>(
                h => _inputActions.Player.AttackMelee.started += h,
                h => _inputActions.Player.AttackMelee.started -= h)
            .Subscribe(_ => AttackMelee.Execute(Unit.Default))
            .AddTo(_disposables);
        Observable.FromEvent<InputAction.CallbackContext>(
                h => _inputActions.Player.Attack.started += h,
                h => _inputActions.Player.Attack.started -= h)
            .Subscribe(_ => AttackRange.Execute(Unit.Default))
            .AddTo(_disposables);
        Observable.FromEvent<InputAction.CallbackContext>(
                h => _inputActions.Player.Interact.started += h,
                h => _inputActions.Player.Interact.started -= h)
            .Subscribe(_ => Interact.Execute(Unit.Default))
            .AddTo(_disposables);
    }

    public void Dispose() {
        _inputActions?.Dispose();
        _disposables?.Dispose();
    }
}