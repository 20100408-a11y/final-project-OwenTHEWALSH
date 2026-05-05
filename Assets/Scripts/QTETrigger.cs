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
    private HIM himScript;
    private GameObject qteText;
    private float qteTimeRemaining = 0f;
    private bool isActive = false;
    private Collider triggerCollider;

    private void Start()
    {
        himScript = FindObjectOfType<HIM>();
        qteText = qteTextGameObject;
        triggerCollider = GetComponent<Collider>();

        if (qteText != null)
            qteText.SetActive(false);

        if (triggerCollider != null)
            triggerCollider.isTrigger = true;

        Debug.Log($"<color=cyan>QTE TRIGGER {triggerNumber} READY</color>");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isActive)
        {
            Debug.Log($"<color=yellow>>>> QTE TRIGGER {triggerNumber} ACTIVATED <<<</color>");
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

        if (himScript != null)
            himScript.ShowHIM("QTE!");

        Debug.Log($"<color=yellow>QTE TRIGGER {triggerNumber}: QTE Active! Press E in {qteWindow} seconds!</color>");
    }

    private void QTESuccess()
    {
        isActive = false;

        if (qteText != null)
            qteText.SetActive(false);

        if (himScript != null)
            himScript.HideTextBox();

        Debug.Log($"<color=green>✓ QTE TRIGGER {triggerNumber}: QTE SUCCESS!</color>");
        gameObject.SetActive(false);
    }

    private void QTETimeout()
    {
        isActive = false;

        if (qteText != null)
            qteText.SetActive(false);

        failedQTECount++;
        Debug.Log($"<color=red>✗ QTE TRIGGER {triggerNumber}: QTE FAILED! ({failedQTECount}/{maxFailedQTEs})</color>");

        if (failedQTECount >= maxFailedQTEs)
        {
            HIMAttackAndGameOver();
        }
        else
        {
            if (himScript != null)
                himScript.HideTextBox();
            gameObject.SetActive(false);
        }
    }

    private void HIMAttackAndGameOver()
    {
        if (himScript != null)
        {
            himScript.ShowHIM("You Failed!");
        }

        Debug.Log($"<color=red>╔════════════════════════════════════╗</color>");
        Debug.Log($"<color=red>║  HIM ATTACK! GAME OVER!            ║</color>");
        Debug.Log($"<color=red>║  Failed QTE Triggers: {failedQTECount}            ║</color>");
        Debug.Log($"<color=red>╚════════════════════════════════════╝</color>");
        
        // Reset counter for next game
        failedQTECount = 0;
        
        EndGame("Failed 3 QTEs - HIM attacked!");
    }

    private void EndGame(string reason)
    {
        GameLoop gameLoop = FindObjectOfType<GameLoop>();
        if (gameLoop != null)
        {
            gameLoop.TriggerGameOver(reason);
        }
    }

    public static int GetFailedQTECount()
    {
        return failedQTECount;
    }

    public static void ResetFailedQTECount()
    {
        failedQTECount = 0;
        Debug.Log($"<color=cyan>QTE Failed count reset to 0</color>");
    }
}
