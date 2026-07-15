using TMPro;
using UnityEngine;

public class ResultUIController : MonoBehaviour
{
    [SerializeField]
    private TMP_Text rankText;

    [SerializeField]
    private TMP_Text timeText;


    private void Start()
    {
        DisplayResult();
    }


    private void DisplayResult()
    {
        Rank rank = ResultEvaluator.Evaluate();

        float time = GameData.Result.Time;


        rankText.text = $"{rank}";

        timeText.text = $"{time:F1}s";
    }
}