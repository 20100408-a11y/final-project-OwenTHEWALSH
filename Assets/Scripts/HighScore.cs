using UnityEngine;
using TMPro;

/// <summary>
/// Tracks and displays a high score. Two modes:
/// - TitleScreen: loads/saves PlayerPrefs and shows the persistent high score.
/// - Gameplay: runtime-only score that resets when the scene starts and does NOT write PlayerPrefs.
/// </summary>
public class HighScore : MonoBehaviour
{
    private const string PlayerPrefsKey = "HighScore";

    public enum Mode
    {
        TitleScreen,
        Gameplay
    }

    [SerializeField] private Mode mode = Mode.Gameplay;
    [SerializeField] private TextMeshProUGUI highScoreText;

    private int highScore = 0;
    private Player player;

    private void Awake()
    {
        // TitleScreen reads saved value; Gameplay starts fresh (reset)
        if (mode == Mode.TitleScreen)
            highScore = PlayerPrefs.GetInt(PlayerPrefsKey, 0);
        else
            highScore = 0;
    }

    private void Start()
    {
        player = FindObjectOfType<Player>();
        UpdateText();
        Debug.Log($"HighScore ({mode}): starting value = {highScore}");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Don't count the input if the player is currently unable to move
            if (player != null && player.IsMovementPaused)
                return;

            highScore++;
            UpdateText();

            // Only save when this instance is on the Title screen
            if (mode == Mode.TitleScreen)
                SaveHighScore();
        }
    }

    private void SaveHighScore()
    {
        PlayerPrefs.SetInt(PlayerPrefsKey, highScore);
        PlayerPrefs.Save();
        Debug.Log($"HighScore ({mode}): saved value = {highScore}");
    }

    private void UpdateText()
    {
        if (highScoreText != null)
            highScoreText.text = $"HighScore: {highScore}";
    }

    // Static accessor for other systems (title UI can call this too)
    public static int GetSavedHighScore()
    {
        return PlayerPrefs.GetInt(PlayerPrefsKey, 0);
    }

    // Call from title UI to overwrite saved high score
    public void OverwriteSavedHighScore(int value)
    {
        if (mode != Mode.TitleScreen)
            return;

        highScore = value;
        SaveHighScore();
        UpdateText();
    }

    // Optional UI button: reset saved high score (only allowed in TitleScreen mode)
    public void ResetSavedHighScore()
    {
        if (mode != Mode.TitleScreen)
            return;

        highScore = 0;
        PlayerPrefs.DeleteKey(PlayerPrefsKey);
        PlayerPrefs.Save();
        UpdateText();
        Debug.Log("HighScore: Reset saved high score");
    }
}