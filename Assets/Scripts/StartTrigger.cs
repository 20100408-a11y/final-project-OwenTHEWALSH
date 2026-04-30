using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StartTrigger : MonoBehaviour
{
    [Tooltip("Camera to detect for trigger.")]
    [SerializeField]
    private Camera targetCamera;

    [Tooltip("HIM GameObject to show when triggered.")]
    [SerializeField]
    private GameObject himGameObject;

    [Tooltip("Text GameObject with dialogue to activate.")]
    [SerializeField]
    private GameObject textGameObject;

    [Tooltip("Dialogue text for HIM's introduction.")]
    [SerializeField]
    private string introductionDialogue = "";

    [Tooltip("GameLoop script reference for climb sequence activation.")]
    [SerializeField]
    private GameObject gameLoopGameObject;

    [Header("Timing Settings")]
    [Tooltip("Delay before intro text appears after HIM is shown (seconds).")]
    [SerializeField]
    private float delayBeforeIntroText = 1.0f;

    [Tooltip("Delay before QTE starts after intro text ends (seconds).")]
    [SerializeField]
    private float delayBeforeQTE = 2.0f;

    [Tooltip("Minimum duration player must stay in trigger (seconds).")]
    [SerializeField]
    private float minimumTriggerDuration = 2.0f;

    [Header("Text Display Settings")]
    [Tooltip("Delay between each word appearing on screen (seconds).")]
    [SerializeField]
    private float wordDisplayDelay = 0.5f;

    [Tooltip("Duration to display the intro text before it disappears (seconds).")]
    [SerializeField]
    private float introTextDisplayDuration = 3.0f;

    private GameLoop gameLoopScript;
    private HIM himScript;

    private bool hasTriggered = false;
    private float triggerTimer = 0f;
    private bool playerInTrigger = false;

    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (himGameObject != null)
        {
            himGameObject.SetActive(false);
            himScript = himGameObject.GetComponent<HIM>();
        }

        if (textGameObject != null)
        {
            textGameObject.SetActive(false);
        }

        if (gameLoopGameObject != null)
        {
            gameLoopScript = gameLoopGameObject.GetComponent<GameLoop>();
        }
        else
        {
            gameLoopScript = FindObjectOfType<GameLoop>();
        }
    }

    private void Update()
    {
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null || targetCamera == null)
            return;

        bool currentlyInTrigger = triggerCollider.bounds.Contains(targetCamera.transform.position);

        // Track time in trigger
        if (currentlyInTrigger)
        {
            if (!playerInTrigger)
            {
                // Just entered trigger
                playerInTrigger = true;
                triggerTimer = 0f;
                
                if (!hasTriggered)
                {
                    TriggerHIM();
                }
            }
            else if (!hasTriggered)
            {
                // Already in trigger, increase timer
                triggerTimer += Time.deltaTime;

                // Check if minimum duration met
                if (triggerTimer >= minimumTriggerDuration)
                {
                    ActivateClimbSequence();
                }
            }
        }
        else
        {
            // Player left trigger
            if (playerInTrigger && !hasTriggered)
            {
                Debug.Log("StartTrigger: Player left trigger before minimum duration!");
                ResetTrigger();
            }
            playerInTrigger = false;
        }
    }

    private void TriggerHIM()
    {
        Debug.Log("StartTrigger: Camera entered trigger zone!");

        // Activate HIM GameObject
        if (himGameObject != null)
        {
            himGameObject.SetActive(true);
            if (himScript != null)
            {
                himScript.ShowHIM(introductionDialogue);
                Debug.Log("StartTrigger: HIM has appeared!");
            }
        }

        // Activate text GameObject with delay
        if (textGameObject != null)
        {
            StartCoroutine(ShowTextWithDelay());
        }
        else
        {
            Debug.LogWarning("StartTrigger: Text GameObject not assigned");
        }
    }

    private IEnumerator ShowTextWithDelay()
    {
        yield return new WaitForSeconds(delayBeforeIntroText);
        
        if (textGameObject != null)
        {
            // Set the text with word-by-word animation
            StartCoroutine(AnimateText(introductionDialogue, wordDisplayDelay));
            Debug.Log("StartTrigger: Text activated!");
        }
    }

    private IEnumerator AnimateText(string text, float delayPerWord)
    {
        TextMeshProUGUI textComponent = textGameObject.GetComponent<TextMeshProUGUI>();
        if (textComponent == null)
        {
            Debug.LogWarning("StartTrigger: TextMeshProUGUI component not found on text GameObject");
            yield break;
        }

        textGameObject.SetActive(true);
        textComponent.text = string.Empty;

        string[] words = text.Split(' ');
        string displayedText = string.Empty;

        foreach (string word in words)
        {
            displayedText += word + " ";
            textComponent.text = displayedText.TrimEnd();
            yield return new WaitForSeconds(delayPerWord);
        }

        yield return new WaitForSeconds(introTextDisplayDuration);
        textGameObject.SetActive(false);
    }

    private void ActivateClimbSequence()
    {
        hasTriggered = true;
        Debug.Log("StartTrigger: Minimum duration reached! Climb sequence starting...");

        // Start climb sequence in GameLoop
        if (gameLoopScript != null)
        {
            gameLoopScript.ActivateClimbSequence(textGameObject, introductionDialogue, delayBeforeQTE);
            Debug.Log("StartTrigger: GameLoop climb sequence activated!");
        }
        else
        {
            Debug.LogWarning("StartTrigger: GameLoop script not assigned");
        }
    }

    private void ResetTrigger()
    {
        Debug.Log("StartTrigger: Resetting trigger...");
        hasTriggered = false;
        triggerTimer = 0f;

        if (himGameObject != null)
        {
            himGameObject.SetActive(false);
        }

        if (textGameObject != null)
        {
            textGameObject.SetActive(false);
        }
    }
}
