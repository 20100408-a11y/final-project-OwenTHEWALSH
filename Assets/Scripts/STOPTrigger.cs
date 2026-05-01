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
    [SerializeField] private string stopDialogue = "STOP";

    private HIM himScript;
    private GameObject stopTextBox;
    private Vector3 lastPlayerPosition;
    private float stopTimer = 0f;
    private bool isActive = false;
    private Collider triggerCollider;

    private void Start()
    {
        himScript = FindObjectOfType<HIM>();
        stopTextBox = stopTextBoxGameObject;
        triggerCollider = GetComponent<Collider>();

        if (stopTextBox != null)
            stopTextBox.SetActive(false);

        if (triggerCollider != null)
            triggerCollider.isTrigger = true;

        Debug.Log($"STOPTrigger {triggerNumber}: Ready");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isActive)
        {
            ActivateSTOP();
        }
    }

    private void Update()
    {
        if (!isActive)
            return;

        // Check if player moved
        if (Camera.main != null)
        {
            float distance = Vector3.Distance(Camera.main.transform.position, lastPlayerPosition);
            if (distance > moveThreshold)
            {
                Debug.Log($"STOPTrigger {triggerNumber}: Player moved during STOP. GAME OVER!");
                EndGame("Player moved during STOP");
                return;
            }
        }

        stopTimer -= Time.deltaTime;

        if (stopTimer <= 0f)
        {
            ExitSTOP();
        }
    }

    private void ActivateSTOP()
    {
        isActive = true;
        stopTimer = stopCheckDuration;
        lastPlayerPosition = Camera.main != null ? Camera.main.transform.position : Vector3.zero;

        // Show STOP text
        if (stopTextBox != null)
        {
            stopTextBox.SetActive(true);
            TextMeshProUGUI stopTextComponent = stopTextBox.GetComponent<TextMeshProUGUI>();
            if (stopTextComponent != null)
                stopTextComponent.text = stopDialogue;
        }

        // Show HIM
        if (himScript != null)
            himScript.ShowHIM(stopDialogue);

        Debug.Log($"STOPTrigger {triggerNumber}: STOP! Don't move!");
    }

    private void ExitSTOP()
    {
        isActive = false;

        if (stopTextBox != null)
            stopTextBox.SetActive(false);

        if (himScript != null)
            himScript.HideTextBox();

        Debug.Log($"STOPTrigger {triggerNumber}: STOP phase ended!");
        gameObject.SetActive(false);
    }

    private void EndGame(string reason)
    {
        GameLoop gameLoop = FindObjectOfType<GameLoop>();
        if (gameLoop != null)
        {
            gameLoop.TriggerGameOver(reason);
        }
    }
}
