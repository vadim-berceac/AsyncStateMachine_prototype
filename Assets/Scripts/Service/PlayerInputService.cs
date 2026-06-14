using System;
using UnityEngine.InputSystem;

public class PlayerInputService: IDisposable
{
    public event Action OnStop;
    private readonly InputActionAsset _asset;
    private readonly InputAction _inputActionStop;
    
    private PlayerInputService(InputActionAsset asset)
    {
        _asset = asset;

        _inputActionStop = _asset.FindAction("Stop");

        _inputActionStop.started += OnStopPerformed;
    }

    private void OnStopPerformed(InputAction.CallbackContext context)
    {
        OnStop?.Invoke();
    }

    public void Dispose()
    {
        _inputActionStop.started -= OnStopPerformed;
    }
}
