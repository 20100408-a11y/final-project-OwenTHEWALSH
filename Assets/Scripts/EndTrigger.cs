using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndTrigger : MonoBehaviour
{
    [Tooltip("Camera to detect for trigger.")]
    [SerializeField]
    private Camera targetCamera;

    [Tooltip("Minimum duration player must stay in trigger (seconds).")]
    [SerializeField]
    private float minimumTriggerDuration = 1.0f;

    [Header("Audio (optional)")]
    [Tooltip("Sound played when end trigger is completed.")]
    [SerializeField] private AudioClip endTriggerClip;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    private bool playerInTrigger = false;
    private float triggerTimer = 0f;
    private bool hasTriggered = false;
    private Player playerScript;

    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        // Find the Player script
        playerScript = FindObjectOfType<Player>();
        if (playerScript == null)
            Debug.LogWarning("[EndTrigger] No Player script found in scene.");
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

                // Pause player movement immediately
                if (playerScript != null)
                {
                    playerScript.PauseMovement(minimumTriggerDuration);
                }
            }
            else if (!hasTriggered)
            {
                // Already in trigger, increase timer
                triggerTimer += Time.deltaTime;

                // Check if minimum duration met
                if (triggerTimer >= minimumTriggerDuration)
                {
                    TriggerEnd();
                }
            }
        }
        else
        {
            // Player left trigger
            if (playerInTrigger && !hasTriggered)
            {
                Debug.Log("EndTrigger: Player left trigger before minimum duration!");
                
                // Resume player movement if they left early
                if (playerScript != null)
                {
                    playerScript.ResumeMovement();
                }

                ResetTrigger();
            }
            playerInTrigger = false;
        }
    }

    private void TriggerEnd()
    {
        hasTriggered = true;
        Debug.Log("EndTrigger: Minimum duration reached! Loading Title Screen...");

        // Play end sound effect if assigned
        PlaySfx(endTriggerClip);

        SceneManager.LoadScene("Title Screen");
    }

    private void ResetTrigger()
    {
        playerInTrigger = false;
        triggerTimer = 0f;
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip == null) return;
        Vector3 pos = (Camera.main != null) ? Camera.main.transform.position : transform.position;
        AudioSource.PlayClipAtPoint(clip, pos, Mathf.Clamp01(sfxVolume));
    }
}
