using UnityEngine;
using UnityEngine.UI;

public class TimeImageDisplay : MonoBehaviour
{
    [SerializeField]
    private Image[] digitImages;

    [SerializeField]
    private Sprite[] sprites;

    public void SetTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        string text = $"{minutes:00}:{seconds:00}";

        for (int i = 0; i < digitImages.Length; i++)
        {
            digitImages[i].sprite = GetSprite(text[i]);
        }
    }

    private Sprite GetSprite(char c)
    {
        return c switch
        {
            '0' => sprites[0],
            '1' => sprites[1],
            '2' => sprites[2],
            '3' => sprites[3],
            '4' => sprites[4],
            '5' => sprites[5],
            '6' => sprites[6],
            '7' => sprites[7],
            '8' => sprites[8],
            '9' => sprites[9],
            ':' => sprites[10],
            _ => sprites[11], // ‹ó”’
        };
    }
}