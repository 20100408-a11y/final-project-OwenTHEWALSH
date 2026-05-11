using System.Collections;
using UnityEngine;
using TMPro;

public class STOPTrigger : MonoBehaviour
{
    [SerializeField] private int triggerNumber = 1;
    [SerializeField] private GameObject stopTextBoxGameObject;
    [SerializeField] private float stopCheckDuration = 2f;
    [SerializeField] private float moveThreshold = 0.1f;

    [Header("Detection")]
    [SerializeField] private Camera targetCamera;

    private static int failedSTOPCount = 0;
    private const int maxFailedSTOPs = 3;

    private GameObject stopTextBox;
    private Vector3 lastPlayerPosition;
    private float stopTimer = 0f;
    private bool isActive = false;
    private bool hasBeenTriggered = false;
    private Collider triggerCollider;
    private bool playerInTrigger = false;

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
    }

    private void Update()
    {
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
                Debug.Log($"[STOPTrigger {triggerNumber}] Player moved during STOP! STRIKE {failedSTOPCount + 1}/{maxFailedSTOPs}");
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

        Debug.Log($"[STOPTrigger {triggerNumber}] STOP active for {stopCheckDuration} seconds.");
    }

    private void STOPStrike()
    {
        isActive = false;
        failedSTOPCount++;

        if (stopTextBox != null)
            stopTextBox.SetActive(false);

        if (failedSTOPCount >= maxFailedSTOPs)
        {
            Debug.Log($"[STOPTrigger] HIM ATTACK! GAME OVER! Failed STOPs: {failedSTOPCount}");
            failedSTOPCount = 0;
            EndGame("Failed 3 STOPs - HIM attacked!");
        }
        else
        {
            Debug.Log($"[STOPTrigger {triggerNumber}] STRIKE {failedSTOPCount}/{maxFailedSTOPs}. Trigger destroyed, game continues.");
            Destroy(gameObject);
        }
    }

    private void EndSTOP()
    {
        isActive = false;
        if (stopTextBox != null)
            stopTextBox.SetActive(false);

        Debug.Log($"[STOPTrigger {triggerNumber}] STOP complete.");
        Destroy(gameObject);
    }

    private void EndGame(string reason)
    {
        if (stopTextBox != null)
            stopTextBox.SetActive(false);

        GameLoop gameLoop = FindObjectOfType<GameLoop>();
        if (gameLoop != null)
            gameLoop.TriggerGameOver(reason);


    }

    public static int GetFailedSTOPCount() => failedSTOPCount;
    public static void ResetFailedSTOPCount()
    {
        failedSTOPCount = 0;
        Debug.Log("[STOPTrigger] STOP Failed count reset to 0");
    }
}
