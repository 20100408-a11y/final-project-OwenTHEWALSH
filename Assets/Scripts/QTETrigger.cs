using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QTETrigger : MonoBehaviour
{
    [SerializeField] private int triggerNumber = 1;
    [SerializeField] private GameObject qteTextGameObject;
    [SerializeField] private float qteWindow = 2f;
    [SerializeField] private int maxFailedQTEs = 3;

    [Header("Detection")]
    [SerializeField] private Camera targetCamera;

    private static int failedQTECount = 0;
    private GameObject qteText;
    private float qteTimeRemaining = 0f;
    private bool isActive = false;
    private bool hasBeenTriggered = false;
    private Collider triggerCollider;
    private bool playerInTrigger = false;

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

        Debug.Log($"[QTETrigger {triggerNumber}] QTE active for {qteWindow} seconds. Press E!");
    }

    private void QTESuccess()
    {
        isActive = false;
        if (qteText != null)
            qteText.SetActive(false);

        Debug.Log($"[QTETrigger {triggerNumber}] QTE SUCCESS.");
        
    }

    private void QTETimeout()
    {
        isActive = false;
        if (qteText != null)
            qteText.SetActive(false);

        failedQTECount++;
        Debug.Log($"[QTETrigger {triggerNumber}] QTE FAILED! ({failedQTECount}/{maxFailedQTEs})");

        if (failedQTECount >= maxFailedQTEs)
        {
            Debug.Log($"[QTETrigger] HIM ATTACK! GAME OVER! Failed QTEs: {failedQTECount}");
            failedQTECount = 0;
            EndGame("Failed 3 QTEs - HIM attacked!");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void EndGame(string reason)
    {
        if (qteText != null)
            qteText.SetActive(false);

        GameLoop gameLoop = FindObjectOfType<GameLoop>();
        if (gameLoop != null)
            gameLoop.TriggerGameOver(reason);

        Destroy(gameObject);
    }

    public static int GetFailedQTECount() => failedQTECount;
    public static void ResetFailedQTECount()
    {
        failedQTECount = 0;
        Debug.Log("[QTETrigger] QTE Failed count reset to 0");
    }
}
