using UnityEngine;
using UnityEngine.UI;

public class SC_BossHpSlider : MonoBehaviour
{
    [Header("HP")]
    [SerializeField]
    private Slider hpSlider;

    [Header("Shield")]
    [SerializeField]
    private Slider shieldSlider;

    private SC_EnemyStatusManager targetBoss;

    private void Update()
    {
        // ボス未取得なら探す
        if (targetBoss == null)
        {
            targetBoss = FindFirstObjectByType<SC_EnemyStatusManager>();

            if (targetBoss == null)
            {
                return;
            }

            // HP初期設定
            hpSlider.maxValue = targetBoss.GetMaxHP();

            // Shield初期設定
            shieldSlider.maxValue = targetBoss.GetMaxBossShield();
        }

        // HP更新
        hpSlider.value = targetBoss.GetHP();

        // Shield更新
        shieldSlider.value = targetBoss.GetCurrentBossShield();
    }
}
