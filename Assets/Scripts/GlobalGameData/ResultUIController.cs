using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultUIController : MonoBehaviour
{

    [SerializeField]
    private TimeImageDisplay timeDisplay;

    [SerializeField]
    private Image rankImage;

    [SerializeField]
    private Sprite[] rankSprites;


    private void Start()
    {
        DisplayResult();
    }


    private void DisplayResult()
    {
        Rank rank = ResultEvaluator.Evaluate();
        rankImage.sprite = GetRankSprite(rank);

        timeDisplay.SetTime(GameData.Result.Time);
    }

    private Sprite GetRankSprite(Rank rank)
    {
        return rank switch
        {
            Rank.S => rankSprites[0],
            Rank.A => rankSprites[1],
            Rank.B => rankSprites[2],
            Rank.C => rankSprites[3],
            Rank.D => rankSprites[4],
            _ => rankSprites[4]
        };
    }
}