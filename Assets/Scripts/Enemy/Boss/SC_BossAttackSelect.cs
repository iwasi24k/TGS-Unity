using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Boss/Attack Select State")]
public class SC_BossAttackSelectState : SC_EnemyBaceState
{
    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        SC_BossAttackController boss = Owner.GetComponent<SC_BossAttackController>();

        if (boss == null)
        {
            Manager.ChangeState(0);
            return;
        }

        BossAttackPattern[] attackPatternList = boss.GetAttackPatternList();

        if (attackPatternList == null || attackPatternList.Length == 0)
        {
            Debug.LogWarning("Bossの攻撃パターンリストが設定されていません。");
            Manager.ChangeState(0);
            return;
        }

        List<BossAttackPattern> usablePatternList = new List<BossAttackPattern>();

        for (int i = 0; i < attackPatternList.Length; i++)
        {
            if (attackPatternList[i] == null) continue;
            if (!attackPatternList[i].GetUsePattern()) continue;

            BossAttackStateType[] stateList = attackPatternList[i].GetStateList();
            if (stateList == null || stateList.Length == 0) continue;

            usablePatternList.Add(attackPatternList[i]);
        }

        if (usablePatternList.Count == 0)
        {
            Debug.LogWarning("使用可能なBoss攻撃パターンがありません。");
            Manager.ChangeState(0);
            return;
        }

        // ランダムに攻撃パターンを選択
        int randomIndex = Random.Range(0, usablePatternList.Count);
        BossAttackPattern selectedPattern = usablePatternList[randomIndex];

        Debug.Log("Boss Attack Pattern : " + selectedPattern.GetPatternName());

        BossAttackStateType[] selectedStateList = selectedPattern.GetStateList();
        int[] attackIndexList = new int[selectedStateList.Length];

        for (int i = 0; i < selectedStateList.Length; i++)
        {
            attackIndexList[i] = (int)selectedStateList[i];
        }

        Manager.StartBossAttackList(attackIndexList);
    }

    public override void UpdateState(GameObject Owner, SC_EnemyStatusManager Manager)
    {
    }

    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        Debug.Log("Boss Attack Select State Exit");
    }
}