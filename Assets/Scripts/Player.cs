using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
    [Header("Camera Move Settings")]
    [Tooltip("Camera to move. If null, Camera.main will be used.")]
    [SerializeField]
    private Camera targetCamera;

    [Header("Smooth Movement")]
    [Tooltip("Duration in seconds for smooth camera movement.")]
    [SerializeField]
    private float moveDuration = 0.3f;

    [Tooltip("Easing curve for smooth movement (optional).")]
    [SerializeField]
    private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Light Settings")]
    [Tooltip("Light that is a child of the camera. Moves with the camera.")]
    [SerializeField]
    private Light cameraLight;

    [Header("Repel Settings")]
    [Tooltip("Radius to detect objects for repelling.")]
    [SerializeField]
    private float repelRadius = 10f;

    [Tooltip("Force applied to repel objects.")]
    [SerializeField]
    private float repelForce = 20f;

    [Tooltip("Tag to identify objects to repel (e.g., 'HIM').")]
    [SerializeField]
    private string repelTag = "HIM";

    // distance to move HIM away when player moves
    private const float repelMoveDistance = 10f;

    // Prevent starting multiple concurrent moves       
    private Coroutine moveCoroutine;

    private const float moveUpAmount = 10f; // Always move up by 10 units

    // Movement state tracking
    private bool isMovementPaused = false;

    // Expose the paused state so other scripts (like Move) can respect it
    public bool IsMovementPaused => isMovementPaused;

    [Header("Idle Warning")]
    [Tooltip("Text element used to show idle warning.")]
    [SerializeField]
    private TextMeshProUGUI idleWarningText;

    [Tooltip("Time (s) of inactivity before showing a warning.")]
    [SerializeField]
    private float idleWarningDelay = 1.5f;

    [Tooltip("Time (s) of inactivity that triggers the idle failure action.")]
    [SerializeField]
    private float idleFailTime = 3f;

    [Tooltip("If true, adds a strike when idle fail occurs (requires StrikeManager).")]
    [SerializeField]
    private bool addStrikeOnIdleFail = true;

    [Tooltip("If true, idle failure triggers an immediate Game Over instead of adding a strike.")]
    [SerializeField]
    private bool idleFailIsGameOver = false;

    [Tooltip("Game Over reason text used when idleFailIsGameOver is true.")]
    [SerializeField]
    private string idleFailGameOverReason = "Player idle - game over";

    [Tooltip("Ominous short warning message displayed before failure.")]
    [SerializeField]
    private string idleWarningMessage = "The shadows are stirring... move.";

    [Tooltip("Ominous failure message shown when idle failure occurs.")]
    [SerializeField]
    private string idleFailMessage = "Silence closes in. You have been taken.";

    private float lastActionTime;
    private bool idleWarningVisible = false;
    private bool idleFailed = false;

    // New: control whether idle detection runs (can be disabled by PauseMovement)
    private bool idleDetectionEnabled = true;
    // New: support nested pauses
    private int pauseCount = 0;

    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            Debug.LogWarning("Player: No camera found to move. Assign a Camera in the Inspector or ensure there is a Camera tagged MainCamera.");
        }

        if (cameraLight == null)
        {
            Debug.LogWarning("Player: No light assigned. Assign a Light child of the camera in the Inspector for best results.");
        }

        lastActionTime = Time.time;
        if (idleWarningText != null)
            idleWarningText.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Only process input if movement is not paused
        if (Input.GetKeyDown(KeyCode.Space) && !isMovementPaused)
        {
            if (targetCamera == null)
                return;

            // Stop any existing movement coroutine
            if (moveCoroutine != null)
                StopCoroutine(moveCoroutine);

            // Start smooth camera movement
            moveCoroutine = StartCoroutine(SmoothMove());
            RegisterAction();
        }

        // Idle detection only when enabled
        if (idleDetectionEnabled)
        {
            float idleDuration = Time.time - lastActionTime;

            if (!idleWarningVisible && idleDuration >= idleWarningDelay && !idleFailed)
            {
                ShowIdleWarning(true);
            }
            else if (idleWarningVisible && idleDuration < idleWarningDelay)
            {
                ShowIdleWarning(false);
            }

            if (!idleFailed && idleDuration >= idleFailTime)
            {
                HandleIdleFail();
            }
        }
    }

    private IEnumerator SmoothMove()
    {
        Vector3 startPosition = targetCamera.transform.position;
        Vector3 targetPosition = startPosition + Vector3.up * moveUpAmount;
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / moveDuration);
            float easedProgress = movementCurve.Evaluate(progress);

            targetCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, easedProgress);

            yield return null;
        }

        // Ensure final position is exact
        targetCamera.transform.position = targetPosition;

        RepelObjects();
    }

    private void RepelObjects()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, repelRadius);

        foreach (Collider collider in hitColliders)
        {
            if (collider.CompareTag(repelTag))
            {
                // Prefer calling HIM.MoveAwayFrom when available so HIM controls its own movement
                HIM him = collider.GetComponent<HIM>();
                if (him != null)
                {
                    him.MoveAwayFrom(transform.position, repelMoveDistance);
                    continue;
                }

                // Fallback to previous rigidbody impulse behaviour if HIM component is not present
                Rigidbody rb = collider.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 repelDirection = (collider.transform.position - transform.position).normalized;
                    rb.velocity = Vector3.zero;
                    rb.AddForce(repelDirection * repelForce, ForceMode.Impulse);
                }
            }
        }
    }

    /// <summary>
    /// Temporarily pauses player movement (for STOP triggers, etc.)
    /// Also disables idle detection while paused.
    /// </summary>
    public void PauseMovement(float duration)
    {
        StartCoroutine(PauseMovementCoroutine(duration));
    }

    private IEnumerator PauseMovementCoroutine(float duration)
    {
        // support nested pauses using a counter
        pauseCount++;
        isMovementPaused = true;
        idleDetectionEnabled = false;

        // Stop any ongoing movement immediately
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        yield return new WaitForSeconds(duration);

        pauseCount--;
        if (pauseCount <= 0)
        {
            pauseCount = 0;
            isMovementPaused = false;
            idleDetectionEnabled = true;
            // reset last action so the idle timer doesn't immediately fire on resume
            lastActionTime = Time.time;
            idleFailed = false;
            if (idleWarningVisible)
                ShowIdleWarning(false);
        }
    }

    /// <summary>
    /// Immediately resume player movement
    /// </summary>
    public void ResumeMovement()
    {
        // immediate resume also clears any pause state for idle detection
        pauseCount = 0;
        isMovementPaused = false;
        idleDetectionEnabled = true;
        lastActionTime = Time.time;
        idleFailed = false;
        if (idleWarningVisible)
            ShowIdleWarning(false);
    }

    /// <summary>
    /// Stop any current, in-progress movement immediately but do NOT disable future input.
    /// Use this when the player hits a STOP trigger and you want the camera to halt
    /// at its current position while still allowing the player to press Space again.
    /// </summary>
    public void StopCurrentMovementImmediate()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }
    }

    public Camera GetCamera()
    {
        return targetCamera;
    }

    // Helper: mark player activity (reset idle timers and hide warnings)
    private void RegisterAction()
    {
        lastActionTime = Time.time;
        idleFailed = false;
        if (idleWarningVisible)
            ShowIdleWarning(false);
    }

    private void ShowIdleWarning(bool show)
    {
        idleWarningVisible = show;
        if (idleWarningText != null)
        {
            idleWarningText.text = show ? idleWarningMessage : "";
            idleWarningText.gameObject.SetActive(show);
        }
    }

    private void HandleIdleFail()
    {
        idleFailed = true;
        // final message (reuses the same text box)
        if (idleWarningText != null)
        {
            idleWarningText.text = idleFailMessage;
            idleWarningText.gameObject.SetActive(true);
        }

        Debug.Log("Player: Idle timeout reached.");

        if (idleFailIsGameOver)
        {
            GameLoop gameLoop = FindObjectOfType<GameLoop>();
            if (gameLoop != null)
            {
                gameLoop.TriggerGameOver(idleFailGameOverReason);
            }
            else
            {
                Debug.LogWarning("Player: idleFailIsGameOver is true but no GameLoop found.");
                // fallback to strikes if configured
                if (addStrikeOnIdleFail && StrikeManager.Instance != null)
                    StrikeManager.Instance.AddStrike();
            }
        }
        else
        {
            if (addStrikeOnIdleFail && StrikeManager.Instance != null)
            {
                StrikeManager.Instance.AddStrike();
            }
        }
    }
}
