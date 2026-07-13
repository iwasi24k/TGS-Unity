using UnityEngine;

public class SC_BossAttackController : MonoBehaviour
{
    [Header("Ref")]
    [Tooltip("プレイヤーのTransform。Prefabの場合はNoneでOK。実行時に自動検索する"), SerializeField] private Transform player;
    [Tooltip("Playerを探す時に使うTag"), SerializeField] private string playerTag = "Player";

    [Header("Melee Wave")]
    [Tooltip("近接円形波動のPrefab"), SerializeField] private GameObject meleeWavePrefab;
    [Tooltip("近接円形波動の半径"), SerializeField] private float meleeWaveRadius = 5.0f;
    [Tooltip("近接円形波動でプレイヤーを吹き飛ばす力"), SerializeField] private float meleeKnockBackPower = 10.0f;

    [Header("Homing Missile")]
    [Tooltip("追従型ミサイルを同時に発射する数"), SerializeField] private int homingMissileCount = 5;
    [Tooltip("プレイヤーを追従する時間"), SerializeField] private float homingTime = 2.0f;
    [Tooltip("追従型ミサイルの移動速度"), SerializeField] private float homingMissileSpeed = 8.0f;

    [Header("Falling Missile")]
    [Tooltip("落下型ミサイルを落とす数"), SerializeField] private int fallingMissileCount = 5;
    [Tooltip("ミサイルが落下するまでの時間"), SerializeField] private float fallingTime = 2.0f;
    [Tooltip("落下型ミサイルを出す間隔"), SerializeField] private float fallingInterval = 0.3f;
    [Tooltip("ボスを中心とした落下地点のランダム範囲"), SerializeField] private float stageRadius = 10.0f;
    [Tooltip("落下型ミサイルが落ちない中心範囲。ボス周辺に落としたくない場合に使う"), SerializeField] private float fallingMissileMinRadius = 5.0f;

    [Header("Rapid Missile")]
    [Tooltip("快速ミサイルを同時に発射する数"), SerializeField] private int rapidMissileCount = 5;
    [Tooltip("生成後、発射まで停止する時間"), SerializeField] private float rapidMissileStartDelay = 0.5f;
    [Tooltip("快速ミサイルの速度"), SerializeField] private float rapidMissileSpeed = 18.0f;

    [Header("Machine Gun")]
    [Tooltip("マシンガンで発射する弾数"), SerializeField] private int machineGunBulletCount = 20;
    [Tooltip("マシンガンの発射間隔"), SerializeField] private float machineGunInterval = 0.08f;
    [Tooltip("マシンガン弾の速度"), SerializeField] private float machineGunMissileSpeed = 12.0f;
    [Tooltip("ランダム方向の角度範囲"), SerializeField] private float machineGunRandomAngle = 45.0f;

    [Header("Split Falling Missile")]
    [Tooltip("増殖ミサイルの落下数"), SerializeField] private int splitFallingMissileCount = 3;
    [Tooltip("増殖ミサイルの落下時間"), SerializeField] private float splitFallingTime = 1.5f;
    [Tooltip("増殖ミサイルの落下間隔"), SerializeField] private float splitFallingInterval = 0.3f;
    [Tooltip("地面に当たった時に周囲へ発射する数"), SerializeField] private int splitChildMissileCount = 8;
    [Tooltip("周囲へ発射するミサイルの速度"), SerializeField] private float splitChildMissileSpeed = 10.0f;
    
    [Header("Summon Limit")]
    [SerializeField] private int maxEnemyCount = 10;

    [Header("Attack Pattern")]
    [Tooltip("ボスが使用する攻撃パターンリスト"), SerializeField] private BossAttackPattern[] attackPatternList;
    
    public Transform GetPlayer()
    {
        if (player != null)
        {
            return player;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObj != null)
        {
            player = playerObj.transform;
        }

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

    public float GetFallingMissileMinRadius()
    {
        return fallingMissileMinRadius;
    }


    public int GetRapidMissileCount()
    { 
        return rapidMissileCount; 
    }

    public float GetRapidMissileStartDelay() 
    { 
        return rapidMissileStartDelay; 
    }

    public float GetRapidMissileSpeed() 
    { 
        return rapidMissileSpeed;
    }


    public int GetMachineGunBulletCount() 
    { 
        return machineGunBulletCount; 
    }

    public float GetMachineGunInterval()
    { 
        return machineGunInterval; 
    }

    public float GetMachineGunMissileSpeed() 
    { 
        return machineGunMissileSpeed;
    }

    public float GetMachineGunRandomAngle() 
    { 
        return machineGunRandomAngle;
    }


    public int GetSplitFallingMissileCount() 
    { 
        return splitFallingMissileCount; 
    }

    public float GetSplitFallingTime() 
    { 
        return splitFallingTime; 
    }

    public float GetSplitFallingInterval() 
    {
        return splitFallingInterval;
    }

    public int GetSplitChildMissileCount() 
    { 
        return splitChildMissileCount; 
    }

    public float GetSplitChildMissileSpeed() 
    {
        return splitChildMissileSpeed;
    }

    public int GetMaxEnemyCount()
    {
        return maxEnemyCount;
    }

    private void Awake()
    {
        AutoFindPlayer();
    }

    private void AutoFindPlayer()
    {
        if (player != null) return;

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObj == null)
        {
            Debug.LogWarning("Playerが見つかりません。Tagを確認してください : " + playerTag);
            return;
        }

        player = playerObj.transform;
    }

    private SC_ObjectPool GetEnemyPool(SC_EnemyObjectPoolManager.EnemyPoolType poolType)
    {
        if (SC_EnemyObjectPoolManager.Instance == null)
        {
            Debug.LogWarning("SC_EnemyObjectPoolManager がSceneにありません。");
            return null;
        }

        return SC_EnemyObjectPoolManager.Instance.GetPool(poolType);
    }

    public SC_ObjectPool GetHomingMissilePool()
    {
        return GetEnemyPool(SC_EnemyObjectPoolManager.EnemyPoolType.HomingMissile);
    }

    public SC_ObjectPool GetFallingMissilePool()
    {
        return GetEnemyPool(SC_EnemyObjectPoolManager.EnemyPoolType.FallingMissile);
    }

    public SC_ObjectPool GetSplitFallingMissilePool()
    {
        return GetEnemyPool(SC_EnemyObjectPoolManager.EnemyPoolType.SplitFallingMissile);
    }

    public SC_ObjectPool GetRapidMissilePool()
    {
        return GetEnemyPool(SC_EnemyObjectPoolManager.EnemyPoolType.RapidMissile);
    }

    public SC_ObjectPool GetStraightMissilePool()
    {
        return GetEnemyPool(SC_EnemyObjectPoolManager.EnemyPoolType.StraightMissile);
    }

    public SC_ObjectPool GetReflectableMissilePool()
    {
        return GetEnemyPool(SC_EnemyObjectPoolManager.EnemyPoolType.ReflectableMissile);
    }

    public SC_ObjectPool GetLaunchVisualFallingMissilePool()
    {
        return GetEnemyPool(SC_EnemyObjectPoolManager.EnemyPoolType.LaunchVisualFallingMissile);
    }

    public SC_ObjectPool GetLaunchVisualSplitFallingMissilePool()
    {
        return GetEnemyPool(SC_EnemyObjectPoolManager.EnemyPoolType.LaunchVisualSplitFallingMissile);
    }

    public SC_ObjectPool GetWarningCirclePool()
    {
        return GetEnemyPool(SC_EnemyObjectPoolManager.EnemyPoolType.WarningCircle);
    }

    public SC_ObjectPool GetWarningSectorPool()
    {
        return GetEnemyPool(SC_EnemyObjectPoolManager.EnemyPoolType.WarningSector);
    }

    public SC_ObjectPool GetWarningRectanglePool()
    {
        return GetEnemyPool(SC_EnemyObjectPoolManager.EnemyPoolType.WarningRectangle);
    }

    public BossAttackPattern[] GetAttackPatternList()
    {
        return attackPatternList;
    }
}