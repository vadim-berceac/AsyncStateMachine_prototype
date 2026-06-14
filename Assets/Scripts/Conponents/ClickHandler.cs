using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class ClickHandler : IDisposable
{
    private readonly Camera _mainCamera;
    private readonly CancellationTokenSource _cts = new();
    private readonly float _maxSampleDistance = 2f;
    private readonly float _maxRayDistance = 100f;
    
    private readonly LayerMask _selectableObjectsLayerMask = ~0;
    private readonly int _walkableAreaMask = NavMesh.GetAreaFromName("Walkable") != -1 
        ? 1 << NavMesh.GetAreaFromName("Walkable") 
        : NavMesh.AllAreas;
    
    public event Action<Vector3> OnClick;
    public event Action<GameObject> OnObjectSelected;

    private ClickHandler(Camera mainCamera)
    {
        _mainCamera = mainCamera;

        OnClick += DebugPosition;
        OnObjectSelected += DebugSelection;

        InitializeAsync(_cts.Token).Forget();
    }

    private async UniTask InitializeAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandleClick();
            }

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }
    }

    private void HandleClick()
    {
        var ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out var objHit, _maxRayDistance, _selectableObjectsLayerMask))
        {
            OnObjectSelected?.Invoke(objHit.collider.gameObject);
            return;
        }

        var pos = GetPositionByCamera(ray);
        if (pos != Vector3.zero)
        {
            OnClick?.Invoke(pos);
        }
    }

    private Vector3 GetPositionByCamera(Ray ray)
    {
        var distance = -ray.origin.y / ray.direction.y;
        var rawPoint = ray.origin + ray.direction * distance;

        if (NavMesh.SamplePosition(rawPoint, out var hit, _maxSampleDistance, _walkableAreaMask))
        {
            var pos = hit.position;
            pos.y = 0;
            return pos;
        }

        return Vector3.zero;
    }

    public void Dispose()
    {
        OnClick -= DebugPosition;
        OnObjectSelected -= DebugSelection;
        _cts.Cancel();
    }

    private void DebugPosition(Vector3 clickPosition)
    {
        Debug.Log($"Move to: {clickPosition}");
    }

    private void DebugSelection(GameObject selected)
    {
        Debug.Log($"Selected: {selected.name}");
    }
}