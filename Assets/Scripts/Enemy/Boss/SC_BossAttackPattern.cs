using UnityEngine;

// ボスの攻撃Stateの種類を定義するEnum
public enum BossAttackStateType
{
    MeleeWave = 2,
    HomingMissile = 3,
    FallingMissile = 4,
    Summon = 5
}

// ボスの攻撃パターンを定義するクラス
[System.Serializable]
public class BossAttackPattern
{
    [Tooltip("この攻撃パターンを使用するか"), SerializeField]
    private bool usePattern = true;

    [Tooltip("攻撃パターン名"), SerializeField]
    private string patternName;

    [Tooltip("この順番で実行する攻撃State"), SerializeField]
    private BossAttackStateType[] stateList;

    public bool GetUsePattern()
    {
        return usePattern;
    }

    public string GetPatternName()
    {
        return patternName;
    }

    public BossAttackStateType[] GetStateList()
    {
        return stateList;
    }
}