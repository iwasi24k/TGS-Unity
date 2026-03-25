using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("HP設定")]
    // 最大HP
    public int maxHP = 10;

    // 現在HP（外から直接触らせない）
    private int currentHP;

    [Header("UI（スライダー）")]
    // HPゲージ（Slider）
    public Slider hpSlider;

    void Start()
    {
        // HPを最大で初期化
        currentHP = maxHP;

        // UIに反映
        UpdateHPUI();
    }

    // ---------------------------
    // ダメージ処理
    // ---------------------------
    public void TakeDamage(int damage)
    {
        // HP減少
        currentHP -= damage;

        // 0〜最大に制限
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        // UI更新
        UpdateHPUI();

        // HP0で死亡
        if (currentHP <= 0)
        {
            Debug.Log("プレイヤー死亡");
        }
    }

    // ---------------------------
    // HPゲージ更新
    // ---------------------------
    void UpdateHPUI()
    {
        if (hpSlider != null)
        {
            // 0〜1の割合に変換して表示
            hpSlider.value = (float)currentHP / maxHP;
        }
    }
}