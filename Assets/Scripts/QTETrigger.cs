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

    private static int failedQTECount = 0;
    private GameObject qteText;
    private float qteTimeRemaining = 0f;
    private bool isActive = false;
    private bool hasBeenTriggered = false;

    private void Awake()
    {
        qteText = qteTextGameObject;
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
            triggerCollider.isTrigger = true;

        if (qteText != null)
            qteText.SetActive(false);

        Debug.Log($"[QTETrigger {triggerNumber}] Ready.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasBeenTriggered && other.CompareTag("Player"))
        {
            hasBeenTriggered = true;
            Debug.Log($"[QTETrigger {triggerNumber}] Player entered trigger.");
            ActivateQTE();
        }
    }

    private void Update()
    {
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
        Destroy(gameObject);
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
