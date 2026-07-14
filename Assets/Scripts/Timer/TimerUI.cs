using TMPro;
using UnityEngine;

public class TimerUI : MonoBehaviour
{
    [SerializeField]
    private GameTimer gameTimer;

    [SerializeField]
    private TMP_Text timerText;

    private void Update()
    {
        float time = gameTimer.CurrentTime;

        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}