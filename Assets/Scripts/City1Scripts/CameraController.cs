using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CameraControllerWithAutoMove : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float panSpeed = 5f;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minZoom = 77.5f;
    [SerializeField] private float maxZoom = 95f;
    [SerializeField] private Vector3 minBounds = new Vector3(-200, 77.5f, -30);
    [SerializeField] private Vector3 maxBounds = new Vector3(150, 200f, 320);

    [Header("Free Movement Settings")]
    [SerializeField] private Vector3 freeModePosition = new Vector3(3f, 95f, 155f);
    [SerializeField] private float lockedYPosition = 80f;
    [SerializeField] private Vector3 freeMoveMinBounds = new Vector3(-200, 50f, -30);
    [SerializeField] private Vector3 freeMoveMaxBounds = new Vector3(150, 150f, 320);

    [Header("UI Elements")]
    public Button freeMoveButton;
    public GameObject cameraMoveIndicator;

    // State variables
    private bool isCameraEnabled = true;
    private bool isFreeMoveEnabled = false;
    private bool isCameraLocked = true;
    private bool isDragging = false;
    private bool isPinching = false;
    private float touchStartTime;
    private const float maxTapDuration = 0.2f;
    private Vector3 dragOrigin;

    public bool IsCameraInteracting => isDragging || isPinching;

    void Start()
    {
        transform.position = new Vector3(3f, lockedYPosition, 155f);

        if (freeMoveButton != null)
        {
            freeMoveButton.onClick.AddListener(ToggleFreeMove);
        }
    }

    void Update()
    {
        UpdateIndicator();

        if (!isCameraEnabled) return;
        if (!isCameraLocked)
        {
            HandleTouchInput();
        }

        ClampCameraPosition();
    }

    private void UpdateIndicator()
    {
        if (cameraMoveIndicator != null &&
            cameraMoveIndicator.activeSelf != IsCameraInteracting)
        {
            cameraMoveIndicator.SetActive(IsCameraInteracting);
        }
    }

    private void HandleTouchInput()
    {
        isDragging = false;
        isPinching = false;

        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStartTime = Time.time;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                if (touch.deltaPosition.magnitude > 10f)
                {
                    isDragging = true;
                    HandleSingleTouch();
                }
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                if (Time.time - touchStartTime < maxTapDuration)
                {
                    isDragging = false;
                }
            }
        }
        else if (Input.touchCount == 2)
        {
            isPinching = true;
            HandlePinchZoom();
        }
    }

    private void HandleSingleTouch()
    {
        if (isPinching) return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Moved)
        {
            Vector3 touchDelta = touch.deltaPosition;
            float panX = -touchDelta.x * panSpeed * Time.deltaTime * 0.5f;
            float panZ = -touchDelta.y * panSpeed * Time.deltaTime * 0.5f;
            transform.Translate(new Vector3(panX, 0, panZ), Space.World);
        }
    }

    private void HandlePinchZoom()
    {
        Touch touch1 = Input.GetTouch(0);
        Touch touch2 = Input.GetTouch(1);

        Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
        Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;

        float prevDistance = Vector2.Distance(touch1PrevPos, touch2PrevPos);
        float currentDistance = Vector2.Distance(touch1.position, touch2.position);

        float zoomDelta = (prevDistance - currentDistance) * zoomSpeed * Time.deltaTime;
        zoomDelta = Mathf.Clamp(zoomDelta, -2f, 2f);

        Vector3 position = transform.position;
        position.y = Mathf.Clamp(position.y + zoomDelta, minZoom, maxZoom);
        transform.position = position;
    }

    private void ClampCameraPosition()
    {
        Vector3 position = transform.position;
        Vector3 min = isFreeMoveEnabled ? freeMoveMinBounds : minBounds;
        Vector3 max = isFreeMoveEnabled ? freeMoveMaxBounds : maxBounds;

        position.x = Mathf.Clamp(position.x, min.x, max.x);
        position.y = Mathf.Clamp(position.y, min.y, max.y);
        position.z = Mathf.Clamp(position.z, min.z, max.z);

        transform.position = position;
    }

    private void ToggleFreeMove()
    {
        isFreeMoveEnabled = !isFreeMoveEnabled;
        isCameraLocked = !isFreeMoveEnabled;

        Vector3 targetPosition = isFreeMoveEnabled ?
            freeModePosition :
            new Vector3(transform.position.x, lockedYPosition, transform.position.z);

        StartCoroutine(SmoothTransition(targetPosition));
    }

    private IEnumerator SmoothTransition(Vector3 targetPosition)
    {
        if (IsCameraInteracting) yield break;

        float duration = 1f;
        float elapsed = 0f;
        Vector3 startPosition = transform.position;

        while (elapsed < duration)
        {
            if (IsCameraInteracting) yield break;

            transform.position = Vector3.Lerp(startPosition, targetPosition,
                Mathf.SmoothStep(0f, 1f, elapsed / duration));

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
    }

    public void MoveToPosition(Vector3 targetPosition)
    {
        StartCoroutine(SmoothTransition(targetPosition));
    }

    public void DisableCamera() => isCameraEnabled = false;
    public void EnableCamera() => isCameraEnabled = true;
}