using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class STOPTrigger : MonoBehaviour
{
    [SerializeField] private int triggerNumber = 1;
    [SerializeField] private GameObject stopTextBoxGameObject;
    [SerializeField] private float stopCheckDuration = 2f;
    [SerializeField] private float moveThreshold = 0.1f;

    private GameObject stopTextBox;
    private Vector3 lastPlayerPosition;
    private float stopTimer = 0f;
    private bool isActive = false;
    private bool hasBeenTriggered = false;

    private void Awake()
    {
        stopTextBox = stopTextBoxGameObject;
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
            triggerCollider.isTrigger = true;

        if (stopTextBox != null)
            stopTextBox.SetActive(false);

        Debug.Log($"[STOPTrigger {triggerNumber}] Ready.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasBeenTriggered && other.CompareTag("Player"))
        {
            hasBeenTriggered = true;
            Debug.Log($"[STOPTrigger {triggerNumber}] Player entered trigger.");
            ActivateSTOP();
        }
    }

    private void Update()
    {
        if (!isActive)
            return;

        stopTimer -= Time.deltaTime;

        // Check for movement
        if (Camera.main != null)
        {
            float distance = Vector3.Distance(Camera.main.transform.position, lastPlayerPosition);
            if (distance > moveThreshold)
            {
                Debug.Log($"[STOPTrigger {triggerNumber}] Player moved during STOP! GAME OVER.");
                EndGame("Player moved during STOP");
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
        lastPlayerPosition = Camera.main != null ? Camera.main.transform.position : Vector3.zero;

        if (stopTextBox != null)
            stopTextBox.SetActive(true);

        Debug.Log($"[STOPTrigger {triggerNumber}] STOP active for {stopCheckDuration} seconds.");
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

        Destroy(gameObject);
    }
}
