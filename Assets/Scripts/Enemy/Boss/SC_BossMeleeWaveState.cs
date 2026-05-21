using UnityEngine;

public class SC_BossAttackController : MonoBehaviour
{
    [Header("Ref")]
    [Tooltip("プレイヤーのTransform"), SerializeField] private Transform player;

    [Header("Melee Wave")]
    [Tooltip("近接円形波動のPrefab"), SerializeField] private GameObject meleeWavePrefab;
    [Tooltip("近接円形波動の半径"), SerializeField] private float meleeWaveRadius = 5.0f;
    [Tooltip("近接円形波動でプレイヤーを吹き飛ばす力"), SerializeField] private float meleeKnockBackPower = 10.0f;

    [Header("Homing Missile")]
    [Tooltip("追従型ミサイルのPrefab"), SerializeField] private GameObject homingMissilePrefab;
    [Tooltip("追従型ミサイルを同時に発射する数"), SerializeField] private int homingMissileCount = 5;
    [Tooltip("プレイヤーを追従する時間"), SerializeField] private float homingTime = 2.0f;
    [Tooltip("追従型ミサイルの移動速度"), SerializeField] private float homingMissileSpeed = 8.0f;

    [Header("Falling Missile")]
    [Tooltip("落下型ミサイルのPrefab"), SerializeField] private GameObject fallingMissilePrefab;
    [Tooltip("落下地点に表示する警告マークPrefab"), SerializeField] private GameObject warningMarkPrefab;
    [Tooltip("落下型ミサイルを落とす数"), SerializeField] private int fallingMissileCount = 5;
    [Tooltip("ミサイルが落下するまでの時間"), SerializeField] private float fallingTime = 2.0f;
    [Tooltip("落下型ミサイルを出す間隔"), SerializeField] private float fallingInterval = 0.3f;
    [Tooltip("ボスを中心とした落下地点のランダム範囲"), SerializeField] private float stageRadius = 10.0f;

    [Header("Summon")]
    [Tooltip("召喚する雑魚敵のPrefab"), SerializeField] private GameObject smallEnemyPrefab;
    [Tooltip("一度に召喚する雑魚敵の数"), SerializeField] private int summonCount = 3;
    [Tooltip("ボスからどれくらい離れた位置に召喚するか"), SerializeField] private float summonRadius = 3.0f;

    public Transform GetPlayer()
    {
        return player;
    }

    public GameObject GetMeleeWavePrefab()
    {
        return meleeWavePrefab;
    }

    public float GetMeleeWaveRadius()
    {
        return meleeWaveRadius;
    }

    public float GetMeleeKnockBackPower()
    {
        return meleeKnockBackPower;
    }

    public GameObject GetHomingMissilePrefab()
    {
        return homingMissilePrefab;
    }

    public int GetHomingMissileCount()
    {
        return homingMissileCount;
    }

    public float GetHomingTime()
    {
        return homingTime;
    }

    public float GetHomingMissileSpeed()
    {
        return homingMissileSpeed;
    }

    public GameObject GetFallingMissilePrefab()
    {
        return fallingMissilePrefab;
    }

    public GameObject GetWarningMarkPrefab()
    {
        return warningMarkPrefab;
    }

    public int GetFallingMissileCount()
    {
        return fallingMissileCount;
    }

    public float GetFallingTime()
    {
        return fallingTime;
    }

    public float GetFallingInterval()
    {
        return fallingInterval;
    }

    public float GetStageRadius()
    {
        return stageRadius;
    }

    public GameObject GetSmallEnemyPrefab()
    {
        return smallEnemyPrefab;
    }

    public int GetSummonCount()
    {
        return summonCount;
    }

    public float GetSummonRadius()
    {
        return summonRadius;
    }
}