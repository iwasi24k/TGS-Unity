using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_ComboSpriteUI : MonoBehaviour
{
    [Header("数字スプライトシート")]
    [SerializeField] private Texture2D numberTexture;

    [Header("数字Prefab")]
    [SerializeField] private GameObject digitPrefab;

    [Header("Combo画像")]
    [SerializeField] private GameObject comboImage;

    private Sprite[] numberSprites = new Sprite[10];

    private List<Image> digitImages = new List<Image>();

    private int columns = 5;
    private int rows = 2;

    private void Awake()
    {
        CreateNumberSprites();
    }

    private void Update()
    {
        if (ComboManager.Instance == null) return;

        int combo = ComboManager.Instance.ComboCount;

        if (combo <= 0)
        {
            comboImage.SetActive(false);

            foreach (Image img in digitImages)
            {
                img.gameObject.SetActive(false);
            }

            return;
        }

        comboImage.SetActive(true);

        string comboStr = combo.ToString();

        while (digitImages.Count < comboStr.Length)
        {
            GameObject obj = Instantiate(digitPrefab, transform);

            obj.transform.SetSiblingIndex(
                comboImage.transform.GetSiblingIndex()
            );

            digitImages.Add(obj.GetComponent<Image>());
        }

        foreach (Image img in digitImages)
        {
            img.gameObject.SetActive(false);
        }

        for (int i = 0; i < comboStr.Length; i++)
        {
            int digit = comboStr[i] - '0';

            digitImages[i].sprite = numberSprites[digit];
            digitImages[i].gameObject.SetActive(true);
        }
    }

    private void CreateNumberSprites()
    {
        int width = numberTexture.width / columns;
        int height = numberTexture.height / rows;

        for (int i = 0; i < 10; i++)
        {
            int x = (i % columns) * width;

            int y = numberTexture.height - ((i / columns) + 1) * height;

            Rect rect = new Rect(x, y, width, height);

            numberSprites[i] = Sprite.Create(
                numberTexture,
                rect,
                new Vector2(0.5f, 0.5f)
            );
        }
    }
}