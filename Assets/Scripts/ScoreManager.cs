using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI bestText;
    int bestScore = 0;

    int score = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        UpdateScore();
    }

    public void AddScore(int amount)
    {
        score += amount;
        if (score > bestScore)
        {
            bestScore = score;
            PlayerPrefs.SetInt("BestScore", bestScore);
        }
        UpdateScore();
    }

    void UpdateScore()
    {
        scoreText.text = "POINTS: " + score;
        bestText.text = "BEST: " + bestScore;
    }
}