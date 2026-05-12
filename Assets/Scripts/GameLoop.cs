using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoop : MonoBehaviour
{
    [Header("Game Over Settings")]
    [SerializeField] private string gameOverSceneName = "Game Over";

    [Header("Audio (optional)")]
    [Tooltip("Sound played when game over is triggered.")]
    [SerializeField] private AudioClip gameOverClip;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    public bool GameOver { get; private set; } = false;

    private static AudioClip staticGameOverClip;
    private static float staticSfxVolume = 1f;

    private void Awake()
    {
        // Store static reference for use in coroutine after this object is destroyed
        staticGameOverClip = gameOverClip;
        staticSfxVolume = sfxVolume;
    }

    public void TriggerGameOver(string reason)
    {
        if (GameOver)
            return;

        GameOver = true;
        Debug.Log($"GameLoop: GAME OVER - {reason}");

        // If a HighScore instance exists in the current scene (gameplay), save its value to PlayerPrefs
        HighScore hs = FindObjectOfType<HighScore>();
        if (hs != null)
        {
            int sessionScore = hs.GetCurrentScore();
            HighScore.SaveIfHigher(sessionScore);
        }

        // Play game over sound effect if assigned
        PlaySfx(gameOverClip, sfxVolume);

        // Load Game Over scene after a short delay
        StartCoroutine(LoadGameOverScene());
    }

    private IEnumerator LoadGameOverScene()
    {
        // Play SFX again at static level in case this object is destroyed before scene loads
        if (staticGameOverClip != null)
            AudioSource.PlayClipAtPoint(staticGameOverClip, Camera.main != null ? Camera.main.transform.position : Vector3.zero, Mathf.Clamp01(staticSfxVolume));

        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(gameOverSceneName);
    }

    private void PlaySfx(AudioClip clip, float volume)
    {
        if (clip == null) return;
        Vector3 pos = (Camera.main != null) ? Camera.main.transform.position : transform.position;
        AudioSource.PlayClipAtPoint(clip, pos, Mathf.Clamp01(volume));
    }
}