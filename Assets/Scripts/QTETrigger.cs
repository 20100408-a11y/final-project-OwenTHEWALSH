using System.Collections;
using UnityEngine;
using TMPro;

public class QTETrigger : MonoBehaviour
{
    [SerializeField] private int triggerNumber = 1;
    [SerializeField] private GameObject qteTextGameObject;
    [SerializeField] private float postTriggerCooldown = 2f;

    [Header("QTE Keys (choose 3)")]
    [Tooltip("Three possible keys the QTE can require (defaults shown).")]
    [SerializeField] private KeyCode[] possibleKeys = new KeyCode[3] { KeyCode.E, KeyCode.R, KeyCode.F };

    [Header("Difficulty (0 = easy, 1 = hard)")]
    [Tooltip("Minimum time allowed to react (hardest).")]
    [SerializeField] private float minQTEWindow = 0.5f;
    [Tooltip("Maximum time allowed to react (easiest).")]
    [SerializeField] private float maxQTEWindow = 3f;
    [Tooltip("Difficulty: 0 = easy (max window), 1 = hard (min window).")]
    [Range(0f, 1f)]
    [SerializeField] private float difficulty = 0.5f;

    [Header("Detection")]
    [SerializeField] private Camera targetCamera;

    [Header("Audio (optional)")]
    [Tooltip("Sound played when QTE begins.")]
    [SerializeField] private AudioClip qteStartClip;
    [Tooltip("Sound played on QTE success.")]
    [SerializeField] private AudioClip qteSuccessClip;
    [Tooltip("Sound played on QTE failure/timeout.")]
    [SerializeField] private AudioClip qteFailClip;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    private GameObject qteText;
    private float qteTimeRemaining = 0f;
    private float cooldownTimer = 0f;
    private bool isActive = false;
    private bool isOnCooldown = false;
    private bool hasBeenTriggered = false;
    private Collider triggerCollider;
    private bool playerInTrigger = false;
    private Player playerScript;

    // Current key required for this QTE instance
    private KeyCode currentRequiredKey = KeyCode.E;
    private TextMeshProUGUI qteTextComponent;

    private void Awake()
    {
        qteText = qteTextGameObject;
        triggerCollider = GetComponent<Collider>();

        if (qteText != null)
        {
            qteText.SetActive(false);
            qteTextComponent = qteText.GetComponent<TextMeshProUGUI>();
        }

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

        // Only accept the required key when this QTE is active
        if (isActive)
        {
            qteTimeRemaining -= Time.deltaTime;

            if (qteTimeRemaining <= 0f)
            {
                QTETimeout();
                return;
            }

            if (Input.GetKeyDown(currentRequiredKey))
            {
                QTESuccess();
            }
        }
    }

    private void ActivateQTE()
    {
        isActive = true;

        // Clamp/validate possible keys
        if (possibleKeys == null || possibleKeys.Length == 0)
            currentRequiredKey = KeyCode.E;
        else
            currentRequiredKey = possibleKeys[Random.Range(0, possibleKeys.Length)];

        // Compute window based on difficulty (higher difficulty => smaller window)
        float clampedDifficulty = Mathf.Clamp01(difficulty);
        qteTimeRemaining = Mathf.Lerp(maxQTEWindow, minQTEWindow, clampedDifficulty);

        if (qteText != null)
        {
            qteText.SetActive(true);
            if (qteTextComponent != null)
            {
                qteTextComponent.text = $"Press [{currentRequiredKey}]";
            }
        }

        // Pause player movement during QTE for the actual window
        if (playerScript != null)
        {
            playerScript.PauseMovement(qteTimeRemaining);
        }

        // Play start SFX if provided
        PlaySfx(qteStartClip);

        Debug.Log($"[QTETrigger {triggerNumber}] QTE active for {qteTimeRemaining:F2}s. Press {currentRequiredKey}!");
    }

    private void QTESuccess()
    {
        isActive = false;
        if (qteText != null)
            qteText.SetActive(false);

        // Play success SFX
        PlaySfx(qteSuccessClip);

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

        // Optionally, play a success visual effect here
    }

    private void QTETimeout()
    {
        isActive = false;
        if (qteText != null)
            qteText.SetActive(false);

        // Play failure SFX
        PlaySfx(qteFailClip);

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

        // Optionally, play a failure visual cue here
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

    private void PlaySfx(AudioClip clip)
    {
        if (clip == null)
            return;

        // Play at main camera if available, otherwise at this object's position.
        Vector3 pos = (Camera.main != null) ? Camera.main.transform.position : transform.position;
        AudioSource.PlayClipAtPoint(clip, pos, Mathf.Clamp01(sfxVolume));
    }
}
