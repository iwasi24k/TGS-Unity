using System;
using UnityEngine;

// ボスの攻撃Stateの種類を定義するEnum
public enum BossAttackStateType
{
    HomingMissile = 3,
    FallingMissile = 4,
    MeleeWave = 5,

    RapidMissile = 6,
    MachineGun = 7,
    SplitFallingMissile = 8,

    CircleBarrageStraight = 9,
    CircleBarrageBackStraight = 10,
    CircleBarrageFrontStraight = 11,
    CircleBarrageFrontBackStraight = 12,
    CircleBarrageLeftStraight = 13,
    CircleBarrageRightStraight = 14,
    CircleBarrageRightLeftStraight = 15,
    CircleBarrageReflectable = 16,
    CircleBarrageBackReflectable = 17,
    CircleBarrageFrontReflectable = 18,
    CircleBarrageFrontBackReflectable = 19,
    CircleBarrageLeftReflectable = 20,
    CircleBarrageRightReflectable = 21,
    CircleBarrageRightLeftReflectable = 22,

    SummonAll = 23,
    SummonRandom = 24,
    SummonByIndex = 25,

    CircleBarragePlayerStraight=26,
    CircleBarragePlayerReflectable = 27,


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
