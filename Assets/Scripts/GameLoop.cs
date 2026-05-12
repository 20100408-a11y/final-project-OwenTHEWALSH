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

    public void TriggerGameOver(string reason)
    {
        if (GameOver)
            return;

        GameOver = true;
        Debug.Log($"GameLoop: GAME OVER - {reason}");

        // Play game over sound effect if assigned
        PlaySfx(gameOverClip);

        // Load Game Over scene after a short delay
        StartCoroutine(LoadGameOverScene());
    }

    private IEnumerator LoadGameOverScene()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(gameOverSceneName);
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip == null) return;
        Vector3 pos = (Camera.main != null) ? Camera.main.transform.position : transform.position;
        AudioSource.PlayClipAtPoint(clip, pos, Mathf.Clamp01(sfxVolume));
    }
}