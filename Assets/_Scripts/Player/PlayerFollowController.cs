using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFollowController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float stopDistance = 0.15f;
    [SerializeField] private Vector2 cursorOffset = new(0f, -1f);

    [Header("Visual")]
    [SerializeField] private Transform visual;
    [SerializeField] private float tiltAmount = 12f;
    [SerializeField] private float tiltSmooth = 10f;

    [Header("Floating")]
    [SerializeField] private float floatAmplitude = 0.12f;
    [SerializeField] private float floatDuration = 1.2f;

    [Header("Sprint")]
    [SerializeField] private bool sprintWithLeftMouse = true;

    [SerializeField] private float screenPadding = 1f;

    private Rigidbody2D rb;
    private Camera cam;

    private Vector2 currentVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }

    private void Start()
    {
        PlayerStats.BaseMoveSpeed = moveSpeed;
        StartFloatingAnimation();
    }

    private void FixedUpdate()
    {
        MoveToMouse();
    }

    private void MoveToMouse()
    {
        if (Mouse.current == null)
            return;

        Vector2 mouseScreenPos =
            Mouse.current.position.ReadValue();

        Vector3 mouseWorld =
            cam.ScreenToWorldPoint(mouseScreenPos);

        mouseWorld.z = 0f;

        Vector2 currentPosition = rb.position;
        Vector2 targetPosition = (Vector2)mouseWorld + cursorOffset;

        Vector2 direction =
            targetPosition - currentPosition;

        float distance = direction.magnitude;

        if (distance <= stopDistance)
        {
            currentVelocity = Vector2.Lerp(
                currentVelocity,
                Vector2.zero,
                acceleration * Time.fixedDeltaTime);

            return;
        }

        direction.Normalize();

        Vector2 targetVelocity =
            direction * GetCurrentMoveSpeed();

        currentVelocity = Vector2.Lerp(
            currentVelocity,
            targetVelocity,
            acceleration * Time.fixedDeltaTime);

        Vector2 nextPosition =
            currentPosition +
            currentVelocity * Time.fixedDeltaTime;

        nextPosition = ClampToScreen(nextPosition);

        rb.MovePosition(nextPosition);

        HandleVisual(direction);
    }

    private float GetCurrentMoveSpeed()
    {
        if (!PlayerStats.SprintUnlocked)
            return PlayerStats.MoveSpeed;

        if (!sprintWithLeftMouse)
            return PlayerStats.MoveSpeed;

        if (Mouse.current == null)
            return PlayerStats.MoveSpeed;

        return Mouse.current.leftButton.isPressed
            ? PlayerStats.SprintMoveSpeed
            : PlayerStats.MoveSpeed;
    }

    private void HandleVisual(Vector2 direction)
    {
        // Flip
        Vector3 scale = visual.localScale;

        if (direction.x > 0.05f)
            scale.x = Mathf.Abs(scale.x);
        else if (direction.x < -0.05f)
            scale.x = -Mathf.Abs(scale.x);

        visual.localScale = scale;

        // Tilt
        float targetTilt =
            -direction.x * tiltAmount;

        Quaternion targetRotation =
            Quaternion.Euler(0f, 0f, targetTilt);

        visual.localRotation = Quaternion.Lerp(
            visual.localRotation,
            targetRotation,
            tiltSmooth * Time.deltaTime);
    }

    private void StartFloatingAnimation()
    {
        visual.DOLocalMoveY(
                floatAmplitude,
                floatDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private Vector2 ClampToScreen(Vector2 targetPosition)
    {
        float verticalSize = cam.orthographicSize;
        float horizontalSize = verticalSize * cam.aspect;

        float padding = screenPadding;

        float minX = cam.transform.position.x - horizontalSize + padding;
        float maxX = cam.transform.position.x + horizontalSize - padding;

        float minY = cam.transform.position.y - verticalSize + padding;
        float maxY = cam.transform.position.y + verticalSize - padding;

        targetPosition.x = Mathf.Clamp(
            targetPosition.x,
            minX,
            maxX);

        targetPosition.y = Mathf.Clamp(
            targetPosition.y,
            minY,
            maxY);

        return targetPosition;
    }
}
