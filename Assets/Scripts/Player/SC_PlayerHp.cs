using UnityEngine;
using UnityEngine.UI;

public class SC_PlayerHP : MonoBehaviour
{
    // =========================================
    // HP設定
    // =========================================

    [Header("HP Settings")]

    [SerializeField] private float maxHP = 100f;
    [SerializeField] private float currentHP;


    // =========================================
    // UI設定
    // =========================================

    [Header("UI Settings")]

    // シーン内のHPスライダー名
    [SerializeField] private string hpSliderObjectName = "HP_Slider";

    // HPバー
    private Slider hpSlider;


    // =========================================
    // ダメージ設定
    // =========================================

    [Header("Damage Settings")]

    [SerializeField] private float bulletDamage = 10f;


    // =========================================
    // 初期化
    // =========================================

    private void Start()
    {
        // HP初期化
        currentHP = maxHP;

        // HPスライダー取得
        FindHPSlider();

        // UI更新
        UpdateHPUI();
    }


    // =========================================
    // HPスライダー取得
    // =========================================

    private void FindHPSlider()
    {
        // 名前でオブジェクト検索
        GameObject sliderObject = GameObject.Find(hpSliderObjectName);

        // 見つかった場合
        if (sliderObject != null)
        {
            hpSlider = sliderObject.GetComponent<Slider>();

            // Slider初期設定
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }
        else
        {
            Debug.LogWarning("HP Slider が見つかりません");
        }
    }


    // =========================================
    // ダメージ処理
    // =========================================

    public void TakeDamage(float damage)
    {
        // HP減少
        currentHP -= damage;

        // HP制限
        currentHP = Mathf.Clamp(currentHP, 0f, maxHP);

        // UI更新
        UpdateHPUI();

    }


    // =========================================
    // UI更新
    // =========================================

    private void UpdateHPUI()
    {
        if (hpSlider != null)
        {
            hpSlider.value = currentHP;
        }
    }


    // =========================================
    // 弾ヒット処理
    // =========================================

    private void OnTriggerEnter(Collider other)
    {
        // Bulletタグ確認
        if (other.CompareTag("Bullet"))
        {
            // ダメージ
            TakeDamage(bulletDamage);

            // 弾削除
            Destroy(other.gameObject);
        }
    }
}