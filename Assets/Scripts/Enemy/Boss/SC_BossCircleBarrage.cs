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

    [Tooltip("1回の発射で出す弾数"), SerializeField]
    private int bulletCount = 16;

    [Tooltip("何回発射するか"), SerializeField]
    private int shotCountMax = 3;

    [Tooltip("発射間隔"), SerializeField]
    private float shotInterval = 0.3f;

    [Tooltip("弾の速度"), SerializeField]
    private float missileSpeed = 8.0f;

    [Tooltip("弾を発射する高さ"), SerializeField]
    private float spawnHeight = 1.2f;

    [Tooltip("ボス中心からどれくらい離して弾を生成するか"), SerializeField]
    private float spawnDistance = 2.5f;

    [Tooltip("発射ごとに角度をずらす量"), SerializeField]
    private float rotateOffsetPerShot = 0.0f;

    [Tooltip("細かい発射角度リスト。空なら360度全方向"), SerializeField]
    private BossBarrageAngleSector[] angleSectorList;

    [Tooltip("攻撃終了後の待ち時間"), SerializeField]
    private float endDelay = 0.5f;

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
        int currentShotIndex
    )
    {
        if (bulletCount <= 0) return;
        if (boss == null) return;

        SC_ObjectPool pool = GetMissilePool(boss);

        if (pool == null) return;

        float rotateOffset =
            rotateOffsetPerShot * currentShotIndex;

        if (angleSectorList == null || angleSectorList.Length == 0)
        {
            FireSector(
                Owner,
                boss,
                pool,
                0f,
                360f,
                bulletCount,
                rotateOffset
            );

            return;
        }

        int usableSectorCount = GetUsableSectorCount();

        if (usableSectorCount <= 0)
        {
            FireSector(
                Owner,
                boss,
                pool,
                0f,
                360f,
                bulletCount,
                rotateOffset
            );

            return;
        }

        int bulletPerSector = Mathf.Max(1, bulletCount / usableSectorCount);

        for (int i = 0; i < angleSectorList.Length; i++)
        {
            BossBarrageAngleSector sector = angleSectorList[i];

            if (sector == null) continue;
            if (!sector.GetUseSector()) continue;

            FireSector(
                Owner,
                boss,
                pool,
                sector.GetCenterAngle(),
                sector.GetAngleRange(),
                bulletPerSector,
                rotateOffset
            );
        }
    }

    private int GetUsableSectorCount()
    {
        if (angleSectorList == null) return 0;

        int count = 0;

        for (int i = 0; i < angleSectorList.Length; i++)
        {
            if (angleSectorList[i] != null &&
                angleSectorList[i].GetUseSector())
            {
                count++;
            }
        }

        return count;
    }

    private void FireSector(
        GameObject Owner,
        SC_BossAttackController boss,
        SC_ObjectPool pool,
        float centerAngle,
        float angleRange,
        int count,
        float rotateOffset
    )
    {
        if (count <= 0) return;

        for (int i = 0; i < count; i++)
        {
            float angle;

            if (angleRange >= 360f)
            {
                angle = 360f / count * i;
            }
            else if (count == 1)
            {
                angle = centerAngle;
            }
            else
            {
                float t = (float)i / (count - 1);

                angle = Mathf.Lerp(
                    centerAngle - angleRange * 0.5f,
                    centerAngle + angleRange * 0.5f,
                    t
                );
            }

            angle += rotateOffset;

            FireOneMissile(
                Owner,
                boss,
                pool,
                angle
            );
        }
    }

    private void FireOneMissile(
    GameObject Owner,
    SC_BossAttackController boss,
    SC_ObjectPool pool,
    float angle)
    {
        if (Owner == null) return;
        if (pool == null) return;

        Vector3 baseDir = GetBaseFireDirection(Owner, boss);

        Vector3 dir =
            Quaternion.Euler(0f, angle, 0f) *
            baseDir;

        dir.y = 0f;

        if (dir.sqrMagnitude <= 0.0001f)
        {
            dir = Owner.transform.forward;
            dir.y = 0f;
        }


        dir.Normalize();

        Vector3 spawnPos = Owner.transform.position + dir * spawnDistance;

        // 地面基準にしたいならこっち
        spawnPos.y = spawnHeight;

        GameObject missileObj = pool.GetObject(
            spawnPos,
            Quaternion.LookRotation(dir)
        );

        if (missileObj == null) return;

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
                            0f
                        );
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
                            missileSpeed
                        );
                    }

                    break;
                }
        }
    }

    private void ShowWarning(GameObject Owner)
    {
        SC_BossAttackController boss = Owner.GetComponent<SC_BossAttackController>();

        if (boss == null) return;

        if (HasUsableSectorList() && showSectorWarningWhenListExists)
        {
            ShowWarningSectors(Owner,boss);
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
        if (boss == null) return;

        SC_ObjectPool pool = boss.GetWarningSectorPool();
        if (pool == null) return;

        for (int i = 0; i < angleSectorList.Length; i++)
        {
            BossBarrageAngleSector sector = angleSectorList[i];

            if (sector == null) continue;
            if (!sector.GetUseSector()) continue;

            ShowOneWarningSector(
                Owner,
                boss,
                pool,
                sector.GetCenterAngle(),
                sector.GetAngleRange()
            );
        }
    }

    private void ShowOneWarningSector(
    GameObject Owner,
    SC_BossAttackController boss,
    SC_ObjectPool pool,
    float centerAngle,
    float angleRange)
    {
        Vector3 spawnPos = Owner.transform.position;
        spawnPos.y = 0.0f;
        spawnPos += Vector3.up * warningHeightOffset;

        Vector3 baseDir = GetBaseFireDirection(Owner, boss);

        Quaternion rotation =
            Quaternion.LookRotation(baseDir);

        GameObject warningObj = pool.GetObject(
            spawnPos,
            rotation
        );

        if (warningObj == null) return;

        SC_WarningTelegraphSector warning =
            warningObj.GetComponent<SC_WarningTelegraphSector>();

        if (warning != null)
        {
            warning.SetPool(pool);
            warning.OnGetFromPool();

            warning.Init(
                sectorWarningRadius,
                centerAngle,
                angleRange,
                warningTime
            );
        }
    }


    private bool HasUsableSectorList()
    {
        if (angleSectorList == null) return false;
        if (angleSectorList.Length == 0) return false;

        for (int i = 0; i < angleSectorList.Length; i++)
        {
            if (angleSectorList[i] == null) continue;

            if (angleSectorList[i].GetUseSector())
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