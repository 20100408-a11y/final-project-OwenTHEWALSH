using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QTETrigger : MonoBehaviour
{
    [SerializeField] private int triggerNumber = 1;
    [SerializeField] private GameObject qteTextGameObject;


    private HIM himScript;
    private GameObject qteText;
    private float qteTimeRemaining = 0f;
    private bool isActive = false;
    private Collider triggerCollider;
    private float qteWindow = 2f; // Time window for QTE in seconds


    private void Start()
    {
        himScript = FindObjectOfType<HIM>();
        qteText = qteTextGameObject;
        triggerCollider = GetComponent<Collider>();

        if (qteText != null)
            qteText.SetActive(false);

        if (triggerCollider != null)
            triggerCollider.isTrigger = true;

        Debug.Log($"QTETrigger {triggerNumber}: Ready");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isActive)
        {
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

        Debug.Log($"QTETrigger {triggerNumber}: QTE Active! Press E in {qteWindow} seconds!");
    }

    private void QTESuccess()
    {
        isActive = false;

        if (qteText != null)
            qteText.SetActive(false);

        if (himScript != null)
            himScript.HideTextBox();

        Debug.Log($"QTETrigger {triggerNumber}: QTE Success!");
        gameObject.SetActive(false);
    }

    private void QTETimeout()
    {
        isActive = false;

        if (qteText != null)
            qteText.SetActive(false);

        Debug.Log($"QTETrigger {triggerNumber}: QTE Failed - Time expired. GAME OVER!");
        EndGame("QTE Failed - Time expired");
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
