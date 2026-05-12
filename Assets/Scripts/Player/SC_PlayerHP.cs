using UnityEngine;
using UnityEngine.UI;

public class SC_PlayerHP : MonoBehaviour
{
    // =========================
    // HP設定
    // =========================

    [Header("HP Settings")]

    // 最大HP
    public float maxHP = 100f;

    // 現在HP
    public float currentHP;


    // =========================
    // UI設定
    // =========================

    [Header("UI")]

    // HPバー用Slider
    public Slider hpSlider;


    // =========================
    // ダメージ設定
    // =========================

    [Header("Damage Settings")]

    // 弾のダメージ量
    public float bulletDamage = 10f;


    // =========================
    // 初期化
    // =========================

    void Start()
    {
        // 開始時に最大HP
        currentHP = maxHP;

        // Slider設定
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }
    }


    // =========================
    // ダメージ処理
    // =========================

    public void TakeDamage(float damage)
    {
        // HP減少
        currentHP -= damage;

        // HPを制限
        currentHP = Mathf.Clamp(currentHP, 0f, maxHP);

        // UI更新
        UpdateHPUI();

    }


    // =========================
    // UI更新
    // =========================

    void UpdateHPUI()
    {
        // Slider更新
        if (hpSlider != null)
        {
            hpSlider.value = currentHP;
        }
    }



    // =========================
    // 弾が当たった時
    // =========================

    private void OnTriggerEnter(Collider other)
    {
        // Bulletタグなら
        if (other.CompareTag("Bullet"))
        {
            // ダメージ
            TakeDamage(bulletDamage);

            // 弾を削除
            Destroy(other.gameObject);
        }
    }
}