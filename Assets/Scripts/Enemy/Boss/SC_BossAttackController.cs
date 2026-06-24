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

    [Header("Missile Pool")]
    [Tooltip("追従型ミサイル用Pool"), SerializeField]
    private SC_ObjectPool homingMissilePool;

    [Tooltip("落下型ミサイル用Pool"), SerializeField]
    private SC_ObjectPool fallingMissilePool;

    [Tooltip("増殖ミサイル用Pool"), SerializeField]
    private SC_ObjectPool splitFallingMissilePool;

    [Tooltip("快速ミサイル用Pool"), SerializeField]
    private SC_ObjectPool rapidMissilePool;

    [Tooltip("直進ミサイル用Pool"), SerializeField]
    private SC_ObjectPool straightMissilePool;

    [Tooltip("反射ミサイル用Pool"), SerializeField]
    private SC_ObjectPool reflectableMissilePool;

    [Header("Warning Pool")]
    [Tooltip("円形警告用Pool"), SerializeField]
    private SC_ObjectPool warningCirclePool;

    [Tooltip("扇形警告用Pool"), SerializeField]
    private SC_ObjectPool warningSectorPool;

    [Tooltip("四角形警告用Pool"), SerializeField]
    private SC_ObjectPool warningRectanglePool;

    [Header("Pool Object Name")]
    [Tooltip("追従型ミサイルPoolのScene上の名前"), SerializeField]
    private string homingMissilePoolObjectName = "PF_HomingMissile Pool";

    [Tooltip("落下型ミサイルPoolのScene上の名前"), SerializeField]
    private string fallingMissilePoolObjectName = "PF_FallingMissile Pool";

    [Tooltip("増殖ミサイルPoolのScene上の名前"), SerializeField]
    private string splitFallingMissilePoolObjectName = "PF_SplitFallingMissile Pool";

    [Tooltip("快速ミサイルPoolのScene上の名前"), SerializeField]
    private string rapidMissilePoolObjectName = "PF_RapidMissile Pool";

    [Tooltip("直進ミサイルPoolのScene上の名前"), SerializeField]
    private string straightMissilePoolObjectName = "PF_StraightMissile Pool";

    [Tooltip("反射ミサイルPoolのScene上の名前"), SerializeField]
    private string reflectableMissilePoolObjectName = "PF_ReflectableMissile Pool";

    [Tooltip("円形Warning PoolのScene上の名前"), SerializeField]
    private string warningCirclePoolObjectName = "PF_WarningCircle Pool";

    [Tooltip("扇形Warning PoolのScene上の名前"), SerializeField]
    private string warningSectorPoolObjectName = "PF_WarningSector Pool";

    [Tooltip("四角形Warning PoolのScene上の名前"), SerializeField]
    private string warningRectanglePoolObjectName = "PF_WarningRectangle Pool";
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
        AutoFindPools();
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

    private void AutoFindPools()
    {
        if (homingMissilePool == null)
        {
            homingMissilePool = FindPool(homingMissilePoolObjectName);
        }

        if (fallingMissilePool == null)
        {
            fallingMissilePool = FindPool(fallingMissilePoolObjectName);
        }

        if (splitFallingMissilePool == null)
        {
            splitFallingMissilePool = FindPool(splitFallingMissilePoolObjectName);
        }

        if (rapidMissilePool == null)
        {
            rapidMissilePool = FindPool(rapidMissilePoolObjectName);
        }

        if (straightMissilePool == null)
        {
            straightMissilePool = FindPool(straightMissilePoolObjectName);
        }

        if(reflectableMissilePool== null)
        {
            reflectableMissilePool = FindPool(reflectableMissilePoolObjectName);
        }

        if (warningCirclePool == null)
        {
            warningCirclePool = FindPool(warningCirclePoolObjectName);
        }

        if (warningSectorPool == null)
        {
            warningSectorPool = FindPool(warningSectorPoolObjectName);
        }

        if (warningRectanglePool == null)
        {
            warningRectanglePool = FindPool(warningRectanglePoolObjectName);
        }
    }

    private SC_ObjectPool FindPool(string poolObjectName)
    {
        if (string.IsNullOrEmpty(poolObjectName))
        {
            return null;
        }

        GameObject poolObj = GameObject.Find(poolObjectName);

        if (poolObj == null)
        {
            Debug.LogWarning("Poolが見つかりません : " + poolObjectName);
            return null;
        }

        SC_ObjectPool pool = poolObj.GetComponent<SC_ObjectPool>();

        if (pool == null)
        {
            Debug.LogWarning("見つけたObjectにSC_ObjectPoolが付いていません : " + poolObjectName);
            return null;
        }

        return pool;
    }

    public SC_ObjectPool GetHomingMissilePool()
    {
        return homingMissilePool;
    }

    public SC_ObjectPool GetFallingMissilePool()
    {
        return fallingMissilePool;
    }

    public SC_ObjectPool GetSplitFallingMissilePool()
    {
        return splitFallingMissilePool;
    }

    public SC_ObjectPool GetRapidMissilePool()
    {
        return rapidMissilePool;
    }

    public SC_ObjectPool GetStraightMissilePool()
    {
        return straightMissilePool;
    }

    public SC_ObjectPool GetReflectableMissilePool()
    {
        return reflectableMissilePool;
    }

    public SC_ObjectPool GetWarningCirclePool()
    {
        return warningCirclePool;
    }

    public SC_ObjectPool GetWarningSectorPool()
    {
        return warningSectorPool;
    }

    public SC_ObjectPool GetWarningRectanglePool()
    {
        return warningRectanglePool;
    }

    public BossAttackPattern[] GetAttackPatternList()
    {
        return attackPatternList;
    }
}