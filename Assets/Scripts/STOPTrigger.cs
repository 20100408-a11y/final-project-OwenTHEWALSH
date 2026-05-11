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

        // Detection like StartTrigger: check camera inside bounds
        if (!hasBeenTriggered && triggerCollider != null && targetCamera != null)
        {
            bool currentlyIn = triggerCollider.bounds.Contains(targetCamera.transform.position);

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
        }

        if (!isActive)
            return;

        stopTimer -= Time.deltaTime;

        // Check for movement
        if (targetCamera != null)
        {
            float distance = Vector3.Distance(targetCamera.transform.position, lastPlayerPosition);
            if (distance > moveThreshold)
            {
                Debug.Log($"[STOPTrigger {triggerNumber}] Player moved during STOP! Adding strike.");
                STOPStrike();
                return;
            }
        }

        if (stopTimer <= 0f)
        {
            EndSTOP();
        }
    }

    private void ActivateSTOP()
    {
        isActive = true;
        stopTimer = stopCheckDuration;
        lastPlayerPosition = targetCamera != null ? targetCamera.transform.position : Vector3.zero;

        if (stopTextBox != null)
            stopTextBox.SetActive(true);

        // Stop any in-progress movement immediately but do NOT disable future input.
        if (playerScript != null)
        {
            playerScript.StopCurrentMovementImmediate();
        }

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
}
