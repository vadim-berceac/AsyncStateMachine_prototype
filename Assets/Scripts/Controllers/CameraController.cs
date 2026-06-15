using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class CameraController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float edgeThickness = 20f; 

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float minOrthoSize = 5f;
    [SerializeField] private float maxOrthoSize = 50f;

    [Header("Drag Pan")]
    [SerializeField] private bool enableDragPan = true;
    [SerializeField] private float dragSpeed = 1f;

    [Header("Camera Clamping")] 
    [SerializeField] private float minX = -5;
    [SerializeField] private float maxX = 5;
    [SerializeField] private float minZ = -5;
    [SerializeField] private float maxZ = 5;

    [SerializeField] private Camera cam;
    private Vector3 _dragOrigin;
    private Vector3 _initialPosition;
    private PlayerInputService _playerInputService;
    private Vector2 _moveInput;
    private Vector2 _scrollInput;
    private Transform _transform;
    
    [Inject]
    private void Construct(PlayerInputService playerInputService)
    {
        _transform = transform;
        _playerInputService = playerInputService;
        
        cam.orthographicSize = minOrthoSize;
        _initialPosition = transform.position;

        _playerInputService.OnMove += HandleMoveInput;
        _playerInputService.OnScroll += HandleScrollInput;
        
        RunLoopAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTaskVoid RunLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            HandleMovement();
            HandleZoom();
            HandleDragPan();
            HandleClamp();
            
            await UniTask.Yield(PlayerLoopTiming.LastUpdate, token);
        }
    }

    private void HandleMoveInput(Vector2 input)
    {
        _moveInput = input;
    }

    private void HandleScrollInput(Vector2 scroll)
    {
        _scrollInput = scroll;
    }

    private float GetMovementFactor()
    {
        return 1f - cam.orthographicSize / maxOrthoSize;
    }

    private void HandleMovement()
    {
        var moveDir = Vector3.zero;

        moveDir.x += _moveInput.x;     
        moveDir.z += _moveInput.y;    

        var mouse = Mouse.current.position.ReadValue();

        if (mouse.x >= Screen.width - edgeThickness)
            moveDir.x += 1f;

        if (mouse.x <= edgeThickness)
            moveDir.x -= 1f;

        if (mouse.y >= Screen.height - edgeThickness)
            moveDir.z += 1f;

        if (mouse.y <= edgeThickness)
            moveDir.z -= 1f;

        if (moveDir.sqrMagnitude <= 0.001f) return;

        moveDir.Normalize();
        _transform.Translate(moveSpeed * GetMovementFactor() * Time.deltaTime * moveDir, Space.World);
    }

    private void HandleZoom()
    {
        var scrollValue = _scrollInput.y;  

        if (Mathf.Abs(scrollValue) <= 0.01f) return;
        
        var newSize = cam.orthographicSize - scrollValue * zoomSpeed;
        cam.orthographicSize = Mathf.Clamp(newSize, minOrthoSize, maxOrthoSize);

        var returnFactor = cam.orthographicSize / maxOrthoSize;
        _transform.position = new Vector3(
            Mathf.Lerp(transform.position.x, _initialPosition.x, returnFactor),
            _transform.position.y,
            Mathf.Lerp(transform.position.z, _initialPosition.z, returnFactor)
        );
           
        _scrollInput = Vector2.zero;
    }

    private void HandleDragPan()
    {
        if (!enableDragPan) return;

        var factor = GetMovementFactor();
        if (factor <= 0f) return;

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            _dragOrigin = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        }

        if (!Mouse.current.rightButton.isPressed) return;
        
        var mouse = Mouse.current.position.ReadValue();
        var difference = _dragOrigin - cam.ScreenToWorldPoint(mouse);
        _transform.position += difference * dragSpeed * factor;
        _dragOrigin = cam.ScreenToWorldPoint(mouse);
    }
    
    private void HandleClamp()
    {
        _transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, minX, maxX),
            _transform.position.y,
            Mathf.Clamp(transform.position.z, minZ, maxZ)
        );
    }

    private void OnDisable()
    {
        _playerInputService.OnMove -= HandleMoveInput;
        _playerInputService.OnScroll -= HandleScrollInput;
    }
}