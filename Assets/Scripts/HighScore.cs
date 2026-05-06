using UnityEngine;
using TMPro;

public class HighScore : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI highScoreText;

    private int highScore = 0;

    private void Start()
    {
        if (highScoreText != null)
            highScoreText.text = "High Score: 0";
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            highScore++;
            if (highScoreText != null)
                highScoreText.text = $"High Score: {highScore}";
        }
    }
}