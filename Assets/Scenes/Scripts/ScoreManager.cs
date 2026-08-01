using UnityEngine;
using TMPro; // Eğer hala hata veriyorsa bu satırı ve aşağıdaki TextMeshProUGUI satırını silip dene

public class ScoreManager : MonoBehaviour
{
    public int totalScore = 0;
    public int streakCount = 0;
    public TextMeshProUGUI scoreText; // Sahnedeki yazıyı buraya sürükleyeceksin

    public void AddScore(int linesCleared) 
    {
        if (linesCleared > 0) 
        {
            streakCount++; 
            int comboMultiplier = linesCleared; 
            int streakMultiplier = Mathf.Max(1, streakCount); 

            int gain = (linesCleared * 100) * comboMultiplier * streakMultiplier;
            totalScore += gain;
            Debug.Log("Puan: " + gain + " | Streak: " + streakCount);
        } 
        else 
        {
            streakCount = 0; 
        }

        if (scoreText != null)
            scoreText.text = "Score: " + totalScore.ToString();
    }
}