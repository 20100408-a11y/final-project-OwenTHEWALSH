using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFX : MonoBehaviour
{
    [Tooltip("Sound effect to play on awake.")]
    [SerializeField] public AudioClip soundClip;
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    void Start()
    {
        AudioSource audioSource = GetComponent<AudioSource>();

        if (soundClip != null)
        {
            if (audioSource != null)
            {
                audioSource.clip = soundClip;
                audioSource.volume = Mathf.Clamp01(volume);
                audioSource.Play();
                Destroy(gameObject, soundClip.length);
            }
            else
            {
                AudioSource.PlayClipAtPoint(soundClip, transform.position, Mathf.Clamp01(volume));
                // Do NOT destroy this GameObject here; PlayClipAtPoint handles cleanup.
            }
        }
        else if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
            Destroy(gameObject, audioSource.clip.length);
        }
    }
}
