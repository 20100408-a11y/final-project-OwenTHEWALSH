using System.Collections;
using UnityEngine;
using TMPro;

public class STOPTrigger : MonoBehaviour
{
    [SerializeField] private int triggerNumber = 1;
    [SerializeField] private GameObject stopTextBoxGameObject;
    [SerializeField] private float stopCheckDuration = 2f;
    [SerializeField] private float moveThreshold = 0.1f;
    [SerializeField] private float postTriggerCooldown = 2f;

    [Header("Detection")]
    [SerializeField] private Camera targetCamera;

    [Header("Audio (optional)")]
    [Tooltip("Sound played when STOP begins.")]
    [SerializeField] private AudioClip stopStartClip;
    [Tooltip("Sound played when player violates STOP (strike).")]
    [SerializeField] private AudioClip stopViolationClip;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    [Header("Spawn Settings")]
    [Tooltip("If true, randomize this trigger's Y position on Awake between minSpawnY and maxSpawnY")]
    [SerializeField] private bool randomizeYOnStart = true;
    [Tooltip("Minimum Y for random spawn")]
    [SerializeField] private float minSpawnY = -180f;
    [Tooltip("Maximum Y for random spawn")]
    [SerializeField] private float maxSpawnY = 100f;

    private GameObject stopTextBox;
    private Vector3 lastPlayerPosition;
    private float stopTimer = 0f;
    private float cooldownTimer = 0f;
    private bool isActive = false;
    private bool isOnCooldown = false;
    private bool hasBeenTriggered = false;
    private Collider triggerCollider;
    private bool playerInTrigger = false;
    private Player playerScript;

    // Track last camera position to detect passing-through the trigger between frames
    private Vector3 lastCameraPosition;

    private void Awake()
    {
        stopTextBox = stopTextBoxGameObject;
        triggerCollider = GetComponent<Collider>();
        if (stopTextBox != null)
            stopTextBox.SetActive(false);

        if (triggerCollider == null)
            Debug.LogWarning($"[STOPTrigger {triggerNumber}] No Collider on trigger object.");

        if (targetCamera == null)
            targetCamera = Camera.main;

        // Randomize Y (keep X/Z)
        if (randomizeYOnStart)
        {
            Vector3 p = transform.position;
            p.y = Random.Range(minSpawnY, maxSpawnY);
            transform.position = p;
        }

        // Initialize last camera position
        if (targetCamera != null)
            lastCameraPosition = targetCamera.transform.position;
        else
            lastCameraPosition = Vector3.zero;

        // Find the Player script
        playerScript = FindObjectOfType<Player>();
        if (playerScript == null)
            Debug.LogWarning($"[STOPTrigger {triggerNumber}] No Player script found in scene.");
    }

    private void Update()
    {
        // Handle cooldown timer
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                isOnCooldown = false;
                Destroy(gameObject); // Destroy trigger after cooldown to prevent reuse
            }
            return;
        }

        // Detection like StartTrigger: check camera inside bounds OR passed through between frames
        if (!hasBeenTriggered && triggerCollider != null && targetCamera != null)
        {
            Vector3 camPos = targetCamera.transform.position;
            bool currentlyIn = IsCameraInsideOrPassedThrough(camPos, lastCameraPosition, triggerCollider.bounds);

            if (currentlyIn)
            {
                if (!playerInTrigger)
                {
                    playerInTrigger = true;
                    hasBeenTriggered = true;
                    Debug.Log($"[STOPTrigger {triggerNumber}] Camera entered trigger.");
                    ActivateSTOP();
                }
            }
            else
            {
                playerInTrigger = false;
            }

            lastCameraPosition = camPos;
        }

        if (!isActive)
            return;

        stopTimer -= Time.deltaTime;

        // New: if player presses Space while STOP is active, treat as a violation/strike
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"[STOPTrigger {triggerNumber}] Player pressed Space during STOP! Adding strike.");
            PlaySfx(stopViolationClip);
            STOPStrike();
            return;
        }

        // Check for movement
        if (targetCamera != null)
        {
            float distance = Vector3.Distance(targetCamera.transform.position, lastPlayerPosition);
            if (distance > moveThreshold)
            {
                Debug.Log($"[STOPTrigger {triggerNumber}] Player moved during STOP! Adding strike.");
                PlaySfx(stopViolationClip);
                STOPStrike();
                return;
            }
        }

        if (stopTimer <= 0f)
        {
            EndSTOP();
        }
    }

    private bool IsCameraInsideOrPassedThrough(Vector3 currentCamPos, Vector3 previousCamPos, Bounds bounds)
    {
        // Direct containment
        if (bounds.Contains(currentCamPos))
            return true;

        // If previous was inside (rare), treat as inside
        if (bounds.Contains(previousCamPos))
            return true;

        // Segment intersection: create ray from previous to current and check bounds intersection within segment length
        Vector3 segment = currentCamPos - previousCamPos;
        float segLen = segment.magnitude;
        if (segLen <= 0f)
            return false;

        Ray r = new Ray(previousCamPos, segment.normalized);
        if (bounds.IntersectRay(r, out float distance))
        {
            if (distance <= segLen + Mathf.Epsilon)
                return true;
        }

        return false;
    }

    private void ActivateSTOP()
    {
        isActive = true;
        stopTimer = stopCheckDuration;
        lastPlayerPosition = targetCamera != null ? targetCamera.transform.position : Vector3.zero;

        if (stopTextBox != null)
            stopTextBox.SetActive(true);

        // Stop any in-progress movement immediately and pause player input & idle detection for duration.
        if (playerScript != null)
        {
            playerScript.StopCurrentMovementImmediate();
            playerScript.PauseMovement(stopCheckDuration);
        }

        PlaySfx(stopStartClip);

        Debug.Log($"[STOPTrigger {triggerNumber}] STOP active for {stopCheckDuration} seconds.");
    }

    private void STOPStrike()
    {
        isActive = false;

        // Add a strike via StrikeManager
        if (StrikeManager.Instance != null)
        {
            StrikeManager.Instance.AddStrike();
            Debug.Log($"[STOPTrigger {triggerNumber}] STRIKE added ({StrikeManager.Instance.Strikes}/{StrikeManager.Instance.MaxStrikes})");
        }

        if (stopTextBox != null)
            stopTextBox.SetActive(false);

        // Resume player movement after strike (keeps backwards compatibility with PauseMovement usage elsewhere)
        if (playerScript != null)
        {
            playerScript.ResumeMovement();
        }

        // If strike manager handled game over, it already called GameLoop. Otherwise start cooldown.
        if (StrikeManager.Instance == null || StrikeManager.Instance.Strikes < StrikeManager.Instance.MaxStrikes)
        {
            Debug.Log($"[STOPTrigger {triggerNumber}] Starting {postTriggerCooldown}s cooldown.");
            StartCooldown();
        }
    }

    private void EndSTOP()
    {
        isActive = false;
        if (stopTextBox != null)
            stopTextBox.SetActive(false);

        Debug.Log($"[STOPTrigger {triggerNumber}] STOP complete. Starting {postTriggerCooldown}s cooldown before player can pass.");
        StartCooldown();
    }

    private void StartCooldown()
    {
        isOnCooldown = true;
        cooldownTimer = postTriggerCooldown;
    }

    private void ResetTrigger()
    {
        hasBeenTriggered = false;
        playerInTrigger = false;
    }

    private void EndGame(string reason)
    {
        if (stopTextBox != null)
            stopTextBox.SetActive(false);

        GameLoop gameLoop = FindObjectOfType<GameLoop>();
        if (gameLoop != null)
            gameLoop.TriggerGameOver(reason);
    }

    // Helper to play SFX at the camera (or this object's) position
    private void PlaySfx(AudioClip clip)
    {
        if (clip == null) return;
        Vector3 pos = (Camera.main != null) ? Camera.main.transform.position : transform.position;
        AudioSource.PlayClipAtPoint(clip, pos, Mathf.Clamp01(sfxVolume));
    }
}
