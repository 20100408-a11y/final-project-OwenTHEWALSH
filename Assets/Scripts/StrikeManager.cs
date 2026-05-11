using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Centralized strike/tally system.
/// - Tracks strikes (default max 3).
/// - Updates a TextMeshProUGUI with a lowercase 'l' repeated for each strike (tally).
/// - Triggers game over via GameLoop when max strikes reached.
/// </summary>
public class StrikeManager : MonoBehaviour
{
    public static StrikeManager Instance { get; private set; }

    [Header("Strike Settings")]
    [SerializeField] private int maxStrikes = 3;

    [Header("UI")]
    [Tooltip("Text element used to display tally as repeated lowercase 'l' characters.")]
    [SerializeField] private TextMeshProUGUI strikeDisplay;

    private int strikes = 0;

    public int Strikes => strikes;
    public int MaxStrikes => maxStrikes;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        UpdateUI();
    }

    public void AddStrike()
    {
        strikes = Mathf.Clamp(strikes + 1, 0, maxStrikes);
        UpdateUI();
        if (strikes >= maxStrikes)
        {
            var gameLoop = FindObjectOfType<GameLoop>();
            if (gameLoop != null)
                gameLoop.TriggerGameOver("Accumulated strikes - HIM attacked!");
        }
    }

    public void RemoveStrike()
    {
        if (strikes <= 0)
            return;
        strikes = Mathf.Clamp(strikes - 1, 0, maxStrikes);
        UpdateUI();
    }

    public void ResetStrikes()
    {
        strikes = 0;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (strikeDisplay != null)
            strikeDisplay.text = new string('l', strikes);
    }
}