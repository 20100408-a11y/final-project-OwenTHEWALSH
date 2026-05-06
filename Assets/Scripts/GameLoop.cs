using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoop : MonoBehaviour
{
    [Header("Game Over Settings")]
    [SerializeField] private string gameOverSceneName = "Game Over";

    public bool GameOver { get; private set; } = false;

    public void TriggerGameOver(string reason)
    {
        if (GameOver)
            return;

        GameOver = true;
        Debug.Log($"GameLoop: GAME OVER - {reason}");

        // Load Game Over scene after a short delay
        StartCoroutine(LoadGameOverScene());
    }

    private IEnumerator LoadGameOverScene()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(gameOverSceneName);
    }
}