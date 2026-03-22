using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("HP設定")]
    public int maxHP = 10;
    private int currentHP;

    [Header("UI")]
    public GameObject hpBarPrefab; // 1個分のバー
    public Transform hpBarParent; // 並べる親

    private List<GameObject> hpBars = new List<GameObject>();

    void Start()
    {
        currentHP = maxHP;

        CreateHPBars();
    }

    // HPバーを生成する
    void CreateHPBars()
    {
        for (int i = 0; i < maxHP; i++)
        {
            GameObject bar = Instantiate(hpBarPrefab, hpBarParent);

            // 横に並べる
            RectTransform rt = bar.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(i * 15, 0);

            hpBars.Add(bar);
        }
    }

    // ダメージ処理
    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        UpdateHPUI();

        if (currentHP <= 0)
        {
            Debug.Log("プレイヤー死亡");
        }
    }

    // HPバー更新（右から消す）
    void UpdateHPUI()
    {
        for (int i = 0; i < hpBars.Count; i++)
        {
            if (i < currentHP)
            {
                hpBars[i].SetActive(true);
            }
            else
            {
                hpBars[i].SetActive(false);
            }
        }
    }
}