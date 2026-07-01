using System;
using System.Reflection;
using UnityEngine;

public enum BarrageMissileType
{
    Straight,
    Reflectable
}

public enum BarrageAimBaseType
{
    BossForward,
    ToPlayer
}

[CreateAssetMenu(menuName = "Enemy/Boss/Circle Barrage State")]
public class SC_BossCircleBarrageState : SC_EnemyBaceState
{
    [Header("Attack")]
    [Tooltip("攻撃開始までの時間"), SerializeField]
    private float startDelay = 0.8f;

    [Tooltip("何回発射するか"), SerializeField]
    private int shotCountMax = 3;

    [Tooltip("発射間隔"), SerializeField]
    private float shotInterval = 0.3f;

    [Tooltip("弾の速度"), SerializeField]
    private float missileSpeed = 8.0f;

    [Tooltip("発射時に下向きへどれくらい傾けるか")]
    [SerializeField] private float downwardPower = 0.35f;

    [Tooltip("このY座標以下になったら、ミサイルのY方向移動を止める")]
    [SerializeField] private float missileGroundY = 0.3f;

    [Tooltip("攻撃終了後の待ち時間"), SerializeField]
    private float endDelay = 0.5f;

    [System.Serializable]
    public class BossBarrageFirePointRange
    {
        [Tooltip("何番目のFirePointから使うか。1ならFirePoint1")]
        [SerializeField] private int startFirePointNumber = 1;

        [Tooltip("何番目のFirePointまで使うか。3ならFirePoint3")]
        [SerializeField] private int endFirePointNumber = 3;

        [Tooltip("この範囲を使うか")]
        [SerializeField] private bool useRange = true;

        public int GetStartIndex()
        {
            return Mathf.Max(0, startFirePointNumber - 1);
        }

        public int GetEndIndex()
        {
            return Mathf.Max(0, endFirePointNumber - 1);
        }

        public bool GetUseRange()
        {
            return useRange;
        }
    }

    [Header("Fire Point Range")]
    [Tooltip("発射に使うFirePoint範囲。例: 1～3 と 7～9")]
    [SerializeField] private BossBarrageFirePointRange[] firePointRangeList;

    [Header("Missile Type")]
    [Tooltip("ミサイルの種類"), SerializeField] private BarrageMissileType missileType = BarrageMissileType.Straight;

    [Header("Warning")]
    [Tooltip("攻撃前に円形警告を表示するか"), SerializeField]
    private bool showCircleWarning = true;

    [Tooltip("警告の半径"), SerializeField]
    private float warningRadius = 8.0f;

    [Tooltip("angleSectorListがある場合、扇形Warningを表示するか"), SerializeField]
    private bool showSectorWarningWhenListExists = true;

    [Tooltip("扇形Warningの半径"), SerializeField]
    private float sectorWarningRadius = 8.0f;

    [Tooltip("警告を地面から少し浮かせる高さ"), SerializeField]
    private float warningHeightOffset = 0.03f;

    [Tooltip("警告の表示時間。基本的にStartDelayと同じにする"), SerializeField]
    private float warningTime = 0.8f;

    [Header("Aim")]
    [Tooltip("発射方向の基準")]
    [SerializeField] private BarrageAimBaseType aimBaseType = BarrageAimBaseType.BossForward;

    private float timer;
    private float shotTimer;
    private float endTimer;
    private int shotCount;
    private bool started;
    private bool finishedFire;

    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        if (ShouldSkipAttackByEnemyCount())
        {
            Manager.ChangeNextBossAttackInList();
            return;
        }

        timer = 0f;
        shotTimer = 0f;
        endTimer = 0f;
        shotCount = 0;
        started = false;
        finishedFire = false;

        RotateBossSoFirePoint1FacesPlayer(Owner);

        ShowWarning(Owner);
    }

    public override void UpdateState(GameObject Owner, SC_EnemyStatusManager Manager)
    {
        timer += Time.deltaTime;

        SC_BossAttackController boss = Owner.GetComponent<SC_BossAttackController>();
        if (boss == null)
        {
            Manager.ChangeNextBossAttackInList();
            return;
        }

        if (!started)
        {
            if (timer < startDelay)
            {
                return;
            }

            started = true;
            shotTimer = shotInterval;
        }

        if (!finishedFire)
        {
            shotTimer += Time.deltaTime;

            if (shotCount < shotCountMax && shotTimer >= shotInterval)
            {
                shotTimer = 0f;

                FireCircleBarrage(
                    Owner,
                    boss,
                    shotCount
                );

                shotCount++;
            }

            if (shotCount >= shotCountMax)
            {
                finishedFire = true;
                endTimer = 0f;
            }

            return;
        }

        endTimer += Time.deltaTime;

        if (endTimer >= endDelay)
        {
            Manager.ChangeNextBossAttackInList();
        }
    }

    public override void Exit(GameObject Owner, SC_EnemyStatusManager Manager)
    {
    }

    private void FireCircleBarrage(
    GameObject Owner,
    SC_BossAttackController boss,
    int currentShotIndex)
    {
        if (boss == null) return;

        SC_ObjectPool pool = GetMissilePool(boss);

        if (pool == null) return;

        FireFromFirePoints(
            Owner,
            boss,
            pool
        );
    }

    private void FireFromFirePoints(
    GameObject Owner,
    SC_BossAttackController boss,
    SC_ObjectPool pool)
    {
        if (Owner == null) return;
        if (pool == null) return;

        SC_EnemyStatusManager statusManager =
            Owner.GetComponent<SC_EnemyStatusManager>();

        if (statusManager == null)
        {
            Debug.LogWarning("SC_EnemyStatusManager がありません : " + Owner.name);
            return;
        }

        Transform[] firePointList = statusManager.GetFirePointList();

        if (firePointList == null || firePointList.Length == 0)
        {
            Debug.LogWarning("FirePointList が未設定です : " + Owner.name);
            return;
        }

        bool fired = false;

        // 範囲指定がない場合は全部発射
        if (firePointRangeList == null || firePointRangeList.Length == 0)
        {
            for (int i = 0; i < firePointList.Length; i++)
            {
                Transform firePoint = firePointList[i];

                if (firePoint == null)
                {
                    Debug.LogWarning("FirePointList[" + i + "] が null です");
                    continue;
                }

                FireOneMissileFromPoint(
                    Owner,
                    boss,
                    pool,
                    firePoint
                );

                fired = true;
            }

            return;
        }

        for (int rangeIndex = 0; rangeIndex < firePointRangeList.Length; rangeIndex++)
        {
            BossBarrageFirePointRange range = firePointRangeList[rangeIndex];

            if (range == null)
            {
                Debug.LogWarning("FirePointRange[" + rangeIndex + "] が null です");
                continue;
            }

            if (!range.GetUseRange())
            {
                Debug.LogWarning("FirePointRange[" + rangeIndex + "] は UseRange が false です");
                continue;
            }

            int startIndex = Mathf.Clamp(
                range.GetStartIndex(),
                0,
                firePointList.Length - 1
            );

            int endIndex = Mathf.Clamp(
                range.GetEndIndex(),
                0,
                firePointList.Length - 1
            );

            Debug.Log(
                "FirePointRange 使用 : " +
                (startIndex + 1) + " ～ " + (endIndex + 1)
            );

            if (startIndex <= endIndex)
            {
                for (int i = startIndex; i <= endIndex; i++)
                {
                    Transform firePoint = firePointList[i];

                    if (firePoint == null)
                    {
                        Debug.LogWarning("FirePointList[" + i + "] が null です");
                        continue;
                    }

                    FireOneMissileFromPoint(
                        Owner,
                        boss,
                        pool,
                        firePoint
                    );

                    fired = true;
                }
            }
            else
            {
                for (int i = startIndex; i < firePointList.Length; i++)
                {
                    Transform firePoint = firePointList[i];

                    if (firePoint == null)
                    {
                        Debug.LogWarning("FirePointList[" + i + "] が null です");
                        continue;
                    }

                    FireOneMissileFromPoint(
                        Owner,
                        boss,
                        pool,
                        firePoint
                    );

                    fired = true;
                }

                for (int i = 0; i <= endIndex; i++)
                {
                    Transform firePoint = firePointList[i];

                    if (firePoint == null)
                    {
                        Debug.LogWarning("FirePointList[" + i + "] が null です");
                        continue;
                    }

                    FireOneMissileFromPoint(
                        Owner,
                        boss,
                        pool,
                        firePoint
                    );

                    fired = true;
                }
            }
        }

        if (!fired)
        {
            Debug.LogWarning(
                "FirePointRangeList はあるが、1発も発射されませんでした : " +
                Owner.name
            );
        }
    }

    private void FireOneMissileFromPoint(
    GameObject Owner,
    SC_BossAttackController boss,
    SC_ObjectPool pool,
    Transform firePoint)
    {
        if (Owner == null) return;
        if (pool == null) return;
        if (firePoint == null) return;

        Vector3 dir = firePoint.position - Owner.transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude <= 0.0001f)
        {
            dir = Owner.transform.forward;
            dir.y = 0f;
        }

        if (dir.sqrMagnitude <= 0.0001f)
        {
            dir = Vector3.forward;
        }

        dir.Normalize();

        dir.y = -downwardPower;
        dir.Normalize();

        Vector3 spawnPos = firePoint.position;


        GameObject missileObj = pool.GetObject(
            spawnPos,
            Quaternion.LookRotation(dir)
        );

        if (missileObj == null)
        {
            Debug.LogWarning("Missile Pool から取得できませんでした");
            return;
        }

        switch (missileType)
        {
            case BarrageMissileType.Straight:
                {
                    SC_StraightMissile straightMissile =
                        missileObj.GetComponent<SC_StraightMissile>();

                    if (straightMissile != null)
                    {
                        straightMissile.SetPool(pool);
                        straightMissile.OnGetFromPool();

                        straightMissile.Init(
                            dir,
                            missileSpeed,
                            0f);

                        straightMissile.SetStopYVelocityNearGround(
                            true,
                            missileGroundY);
                    }

                    break;
                }

            case BarrageMissileType.Reflectable:
                {
                    SC_ReflectableMissile reflectableMissile =
                        missileObj.GetComponent<SC_ReflectableMissile>();

                    if (reflectableMissile != null)
                    {
                        reflectableMissile.SetPool(pool);
                        reflectableMissile.OnGetFromPool();

                        reflectableMissile.Init(
                            dir,
                            missileSpeed);

                        reflectableMissile.SetStopYVelocityNearGround(
                            true,
                            missileGroundY);
                    }

                    break;
                }
        }
    }

    private void ShowWarning(GameObject Owner)
    {
        SC_BossAttackController boss = Owner.GetComponent<SC_BossAttackController>();

        if (boss == null) return;

        if (HasUsableFirePointRangeList() && showSectorWarningWhenListExists)
        {
            ShowWarningSectors(Owner, boss);
            return;
        }

        ShowWarningCircle(Owner, boss);
    }

    private void ShowWarningCircle(GameObject Owner, SC_BossAttackController boss)
    {
        if (!showCircleWarning) return;

        if (boss == null) return;

        SC_ObjectPool pool = boss.GetWarningCirclePool();
        if (pool == null) return;

        Vector3 spawnPos = Owner.transform.position;
        spawnPos.y = 0.0f;
        spawnPos += Vector3.up * warningHeightOffset;


        GameObject warningObj = pool.GetObject(
            spawnPos,
            Quaternion.identity
        );

        if (warningObj == null) return;

        SC_WarningTelegraphCircle warning =
            warningObj.GetComponent<SC_WarningTelegraphCircle>();

        if (warning != null)
        {
            warning.SetPool(pool);
            warning.OnGetFromPool();
            warning.Init(
                warningRadius,
                warningTime
            );
        }
    }

    private void ShowWarningSectors(GameObject Owner, SC_BossAttackController boss)
    {
        if (Owner == null) return;
        if (boss == null) return;

        SC_ObjectPool pool = boss.GetWarningSectorPool();
        if (pool == null) return;

        SC_EnemyStatusManager statusManager =
            Owner.GetComponent<SC_EnemyStatusManager>();

        if (statusManager == null) return;

        Transform[] firePointList = statusManager.GetFirePointList();

        if (firePointList == null || firePointList.Length == 0)
        {
            ShowWarningCircle(Owner, boss);
            return;
        }

        // 範囲指定がない場合は円Warningに戻す
        if (firePointRangeList == null || firePointRangeList.Length == 0)
        {
            ShowWarningCircle(Owner, boss);
            return;
        }

        for (int rangeIndex = 0; rangeIndex < firePointRangeList.Length; rangeIndex++)
        {
            BossBarrageFirePointRange range = firePointRangeList[rangeIndex];

            if (range == null) continue;
            if (!range.GetUseRange()) continue;

            int startIndex = Mathf.Clamp(
                range.GetStartIndex(),
                0,
                firePointList.Length - 1
            );

            int endIndex = Mathf.Clamp(
                range.GetEndIndex(),
                0,
                firePointList.Length - 1
            );

            Transform startFirePoint = firePointList[startIndex];
            Transform endFirePoint = firePointList[endIndex];

            if (startFirePoint == null) continue;
            if (endFirePoint == null) continue;

            ShowOneWarningSector(
                Owner,
                boss,
                pool,
                startFirePoint,
                endFirePoint
            );
        }
    }

    private void ShowOneWarningSector(
    GameObject Owner,
    SC_BossAttackController boss,
    SC_ObjectPool pool,
    Transform startFirePoint,
    Transform endFirePoint)
    {
        if (Owner == null) return;
        if (pool == null) return;
        if (startFirePoint == null) return;
        if (endFirePoint == null) return;

        Vector3 spawnPos = Owner.transform.position;
        spawnPos.y = 0.0f;
        spawnPos += Vector3.up * warningHeightOffset;

        GameObject warningObj = pool.GetObject(
            spawnPos,
            Quaternion.identity
        );

        if (warningObj == null) return;

        SC_WarningTelegraphSector warning =
            warningObj.GetComponent<SC_WarningTelegraphSector>();

        if (warning == null) return;

        warning.SetPool(pool);
        warning.OnGetFromPool();

        warning.InitByWorldPoints(
            Owner.transform.position,
            startFirePoint.position,
            endFirePoint.position,
            sectorWarningRadius,
            warningTime
        );
    }

    private Vector3 GetAimRotatedDirectionFromFirePoint(
    GameObject Owner,
    SC_BossAttackController boss,
    Transform firePoint)
    {
        if (Owner == null) return Vector3.forward;
        if (firePoint == null) return Vector3.forward;

        Vector3 ownerForward = Owner.transform.forward;
        ownerForward.y = 0f;

        if (ownerForward.sqrMagnitude <= 0.0001f)
        {
            ownerForward = Vector3.forward;
        }

        ownerForward.Normalize();

        Vector3 firePointDir =
            firePoint.position - Owner.transform.position;

        firePointDir.y = 0f;

        if (firePointDir.sqrMagnitude <= 0.0001f)
        {
            return ownerForward;
        }

        firePointDir.Normalize();

        // Boss正面から見て、このFirePointが何度ズレているか
        float localAngle =
            Vector3.SignedAngle(
                ownerForward,
                firePointDir,
                Vector3.up
            );

        // aimBaseType による基準方向
        // BossForwardならBoss正面
        // ToPlayerならプレイヤー方向
        Vector3 baseDir = GetBaseFireDirection(Owner, boss);
        baseDir.y = 0f;

        if (baseDir.sqrMagnitude <= 0.0001f)
        {
            baseDir = ownerForward;
        }

        baseDir.Normalize();

        // FirePointの相対角度を、基準方向へ回転して適用
        Vector3 rotatedDir =
            Quaternion.Euler(0f, localAngle, 0f) *
            baseDir;

        rotatedDir.y = 0f;

        if (rotatedDir.sqrMagnitude <= 0.0001f)
        {
            return baseDir;
        }

        return rotatedDir.normalized;
    }

    private void RotateBossSoFirePoint1FacesPlayer(GameObject Owner)
    {
        if (Owner == null) return;

        if (aimBaseType != BarrageAimBaseType.ToPlayer)
        {
            return;
        }

        SC_BossAttackController boss =
            Owner.GetComponent<SC_BossAttackController>();

        if (boss == null) return;

        Transform player = boss.GetPlayer();

        if (player == null) return;

        SC_EnemyStatusManager statusManager =
            Owner.GetComponent<SC_EnemyStatusManager>();

        if (statusManager == null) return;

        Transform[] firePointList = statusManager.GetFirePointList();

        if (firePointList == null || firePointList.Length == 0) return;

        Transform firePoint1 = firePointList[0];

        if (firePoint1 == null) return;

        Vector3 firePoint1Dir =
            firePoint1.position - Owner.transform.position;

        firePoint1Dir.y = 0f;

        if (firePoint1Dir.sqrMagnitude <= 0.0001f) return;

        firePoint1Dir.Normalize();

        Vector3 toPlayer =
            player.position - Owner.transform.position;

        toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude <= 0.0001f) return;

        toPlayer.Normalize();

        float angle =
            Vector3.SignedAngle(
                firePoint1Dir,
                toPlayer,
                Vector3.up
            );

        Owner.transform.rotation =
            Quaternion.AngleAxis(angle, Vector3.up) *
            Owner.transform.rotation;
    }

    private bool HasUsableFirePointRangeList()
    {
        if (firePointRangeList == null) return false;
        if (firePointRangeList.Length == 0) return false;

        for (int i = 0; i < firePointRangeList.Length; i++)
        {
            if (firePointRangeList[i] == null) continue;

            if (firePointRangeList[i].GetUseRange())
            {
                return true;
            }
        }

        return false;
    }

    private SC_ObjectPool GetMissilePool(SC_BossAttackController boss)
    {
        if (boss == null) return null;

        switch (missileType)
        {
            case BarrageMissileType.Straight:
                return boss.GetStraightMissilePool();

            case BarrageMissileType.Reflectable:
                return boss.GetReflectableMissilePool();
        }

        return null;
    }

    private Vector3 GetBaseFireDirection(
    GameObject Owner,
    SC_BossAttackController boss)
    {
        if (Owner == null)
        {
            return Vector3.forward;
        }

        switch (aimBaseType)
        {
            case BarrageAimBaseType.ToPlayer:
                {
                    if (boss == null) break;

                    Transform player = boss.GetPlayer();
                    if (player == null) break;

                    Vector3 dir = player.position - Owner.transform.position;
                    dir.y = 0f;

                    if (dir.sqrMagnitude > 0.0001f)
                    {
                        return dir.normalized;
                    }

                    break;
                }

            case BarrageAimBaseType.BossForward:
            default:
                break;
        }

        Vector3 forward = Owner.transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude <= 0.0001f)
        {
            return Vector3.forward;
        }

        return forward.normalized;
    }

    private bool ShouldSkipAttackByEnemyCount()
    {
        SC_EnemyManager enemyManager =
            FindFirstObjectByType<SC_EnemyManager>();

        if (enemyManager == null)
        {
            return false;
        }

        Debug.Log("EnemyCount:"+enemyManager.GetEnemyCount());
        return enemyManager.GetEnemyCount() >= 2;
    }
}