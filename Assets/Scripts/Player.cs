using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    /// </summary>
    public void PauseMovement(float duration)
    {
        StartCoroutine(PauseMovementCoroutine(duration));
    }

    private IEnumerator PauseMovementCoroutine(float duration)
    {
        isMovementPaused = true;
        
        // Stop any ongoing movement
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        yield return new WaitForSeconds(duration);
        
        isMovementPaused = false;
    }

    /// <summary>
    /// Immediately resume player movement
    /// </summary>
    public void ResumeMovement()
    {
        isMovementPaused = false;
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
}
