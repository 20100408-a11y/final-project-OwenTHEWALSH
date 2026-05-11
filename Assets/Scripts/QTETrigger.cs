using System.Collections;
using UnityEngine;

// Add this using directive at the top of your file
// (if StrikeManager is in a namespace, use that namespace)

public class QTETrigger : MonoBehaviour
{
    [SerializeField] private int triggerNumber = 1;
    [SerializeField] private GameObject qteTextGameObject;
    [SerializeField] private float qteWindow = 2f;
    [SerializeField] private float postTriggerCooldown = 2f;

    [Header("Detection")]
    [SerializeField] private Camera targetCamera;

    private GameObject qteText;
    private float qteTimeRemaining = 0f;
    private float cooldownTimer = 0f;
    private bool isActive = false;
    private bool isOnCooldown = false;
    private bool hasBeenTriggered = false;
    private Collider triggerCollider;
    private bool playerInTrigger = false;
    private Player playerScript;

    private void Awake()
    {
        qteText = qteTextGameObject;
        triggerCollider = GetComponent<Collider>();

        if (qteText != null)
            qteText.SetActive(false);

        if (triggerCollider == null)
            Debug.LogWarning($"[QTETrigger {triggerNumber}] No Collider on trigger object.");

        if (targetCamera == null)
            targetCamera = Camera.main;

        // Find the Player script
        playerScript = FindObjectOfType<Player>();
        if (playerScript == null)
            Debug.LogWarning($"[QTETrigger {triggerNumber}] No Player script found in scene.");
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
                    Debug.Log($"[QTETrigger {triggerNumber}] Camera entered trigger.");
                    ActivateQTE();
                }
            }
            else
            {
                playerInTrigger = false;
            }
        }

        if (!isActive)
            return;

        qteTimeRemaining -= Time.deltaTime;

        if (qteTimeRemaining <= 0f)
        {
            QTETimeout();
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            QTESuccess();
        }
    }

    private void ActivateQTE()
    {
        isActive = true;
        qteTimeRemaining = qteWindow;

        if (qteText != null)
            qteText.SetActive(true);

        // Pause player movement during QTE
        if (playerScript != null)
        {
            playerScript.PauseMovement(qteWindow);
        }

        Debug.Log($"[QTETrigger {triggerNumber}] QTE active for {qteWindow} seconds. Press E!");
    }

    private void QTESuccess()
    {
        isActive = false;
        if (qteText != null)
            qteText.SetActive(false);

        // If player has strikes, successful QTE removes one strike.
        if (StrikeManager.Instance != null && StrikeManager.Instance.Strikes > 0)
        {
            StrikeManager.Instance.RemoveStrike();
            Debug.Log($"[QTETrigger {triggerNumber}] QTE SUCCESS - removed a strike. ({StrikeManager.Instance.Strikes}/{StrikeManager.Instance.MaxStrikes})");
        }
        else
        {
            Debug.Log($"[QTETrigger {triggerNumber}] QTE SUCCESS.");
        }

        // Resume player movement after success
        if (playerScript != null)
        {
            playerScript.ResumeMovement();
        }

        Debug.Log($"[QTETrigger {triggerNumber}] Starting {postTriggerCooldown}s cooldown before player can pass.");
        StartCooldown();
    }

    private void QTETimeout()
    {
        isActive = false;
        if (qteText != null)
            qteText.SetActive(false);

        // Resume player movement after timeout
        if (playerScript != null)
        {
            playerScript.ResumeMovement();
        }

        // Add a strike on failure
        if (StrikeManager.Instance != null)
        {
            StrikeManager.Instance.AddStrike();
            Debug.Log($"[QTETrigger {triggerNumber}] QTE FAILED! Added strike ({StrikeManager.Instance.Strikes}/{StrikeManager.Instance.MaxStrikes})");
        }

        // If strikes triggered game over, StrikeManager will handle GameLoop call.
        // Otherwise start cooldown
        if (StrikeManager.Instance == null || StrikeManager.Instance.Strikes < StrikeManager.Instance.MaxStrikes)
        {
            Debug.Log($"[QTETrigger {triggerNumber}] Starting {postTriggerCooldown}s cooldown.");
            StartCooldown();
        }
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
        if (qteText != null)
            qteText.SetActive(false);

        GameLoop gameLoop = FindObjectOfType<GameLoop>();
        if (gameLoop != null)
            gameLoop.TriggerGameOver(reason);
    }
}
