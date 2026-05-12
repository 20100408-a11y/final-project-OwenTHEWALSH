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

    [Header("Text Display Settings")]
    [Tooltip("Delay between each word appearing on screen (seconds).")]
    [SerializeField]
    private float wordDisplayDelay = 0.5f;

    [Tooltip("Duration to display the intro text before it disappears (seconds).")]
    [SerializeField]
    private float introTextDisplayDuration = 3.0f;

    [Header("Audio (optional)")]
    [Tooltip("Sound played when intro starts.")]
    [SerializeField] private AudioClip introStartClip;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    private HIM himScript;
    private Player playerScript;

    private bool hasTriggered = false;
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

        // Find the Player script
        playerScript = FindObjectOfType<Player>();
        if (playerScript == null)
            Debug.LogWarning("[StartTrigger] No Player script found in scene.");
    }

    private void Update()
    {
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null || targetCamera == null)
            return;

        bool currentlyInTrigger = triggerCollider.bounds.Contains(targetCamera.transform.position);

        // Track trigger
        if (currentlyInTrigger)
        {
            if (!playerInTrigger)
            {
                // Just entered trigger
                playerInTrigger = true;
                
                if (!hasTriggered)
                {
                    TriggerHIM();
                }
            }
        }
        else
        {
            playerInTrigger = false;
        }
    }

    private void TriggerHIM()
    {
        hasTriggered = true;
        Debug.Log("StartTrigger: Camera entered trigger zone!");

        // Play intro sound effect if assigned
        PlaySfx(introStartClip);

        // Calculate total pause duration (text animation + display duration)
        float totalPauseDuration = (introductionDialogue.Split(' ').Length * wordDisplayDelay) + introTextDisplayDuration;

        // Pause player movement for the duration of the intro sequence
        if (playerScript != null)
        {
            playerScript.PauseMovement(totalPauseDuration);
        }

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

        // Activate text GameObject immediately
        if (textGameObject != null)
        {
            StartCoroutine(AnimateText(introductionDialogue, wordDisplayDelay));
            Debug.Log("StartTrigger: Text activated!");
        }
        else
        {
            Debug.LogWarning("StartTrigger: Text GameObject not assigned");
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

    private void PlaySfx(AudioClip clip)
    {
        if (clip == null) return;
        Vector3 pos = (Camera.main != null) ? Camera.main.transform.position : transform.position;
        AudioSource.PlayClipAtPoint(clip, pos, Mathf.Clamp01(sfxVolume));
    }
}
