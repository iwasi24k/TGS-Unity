using UnityEngine;
using UnityEngine.UI;

public class SC_BossHpSlider : MonoBehaviour
{
    [Header("HPバー（左→右）")]
    [SerializeField]
    private Image[] hpBars;

    [Header("Shield")]
    [SerializeField]
    private Image shieldFill;

    private SC_EnemyStatusManager targetBoss;

    private void Update()
    {
        if (targetBoss == null)
        {
            targetBoss = FindFirstObjectByType<SC_EnemyStatusManager>();

            if (targetBoss == null)
                return;
        }

        UpdateHP();
        UpdateShield();
    }

    private void UpdateHP()
    {
        float maxHP = targetBoss.GetMaxHP();
        float currentHP = targetBoss.GetHP();

        int partCount = hpBars.Length;

        float partHP = maxHP / partCount;

        for (int i = 0; i < partCount; i++)
        {
            float start = maxHP - (partHP * i);
            float end = start - partHP;

            if (currentHP >= start)
            {
                hpBars[i].fillAmount = 1f;
            }
            else if (currentHP <= end)
            {
                hpBars[i].fillAmount = 0f;
            }
            else
            {
                hpBars[i].fillAmount =
                    (currentHP - end) / partHP;
            }
        }
    }

    private void UpdateShield()
    {
        float maxShield = targetBoss.GetMaxBossShield();

        if (maxShield <= 0)
        {
            shieldFill.fillAmount = 0;
            return;
        }

        shieldFill.fillAmount =
            targetBoss.GetCurrentBossShield() /
            maxShield;
    }
}