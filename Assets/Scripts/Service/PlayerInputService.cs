using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputService: IDisposable
{
    public event Action<Vector2> OnMove;
    public event Action<Vector2> OnScroll;
    private readonly InputAction _inputActionMove;
    private readonly InputAction _inputActionScroll;
    
    private PlayerInputService(InputActionAsset asset)
    {
        _inputActionMove = asset.FindAction("Move");
        _inputActionScroll = asset.FindAction("ScrollWheel");

        _inputActionMove.performed += OnMovePerformed;
        _inputActionMove.canceled += OnMoveCanceled;

        _inputActionScroll.performed += OnScrollWheelPerformed;
        _inputActionScroll.canceled += OnScrollWheelCanceled;
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        OnMove?.Invoke(context.ReadValue<Vector2>());
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        OnMove?.Invoke(Vector2.zero);
    }

    private void OnScrollWheelPerformed(InputAction.CallbackContext context)
    {
        OnScroll?.Invoke(context.ReadValue<Vector2>());
    }
    
    private void OnScrollWheelCanceled(InputAction.CallbackContext context)
    {
        OnScroll?.Invoke(Vector2.zero);
    }

    public void Dispose()
    {
        _inputActionMove.performed -= OnMovePerformed;
        _inputActionMove.canceled -= OnMoveCanceled;
        
        _inputActionScroll.performed -= OnScrollWheelPerformed;
        _inputActionScroll.canceled -= OnScrollWheelCanceled;
    }
}
