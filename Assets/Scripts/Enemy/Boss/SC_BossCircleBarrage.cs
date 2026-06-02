using System;
using UnityEngine;

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

    private float timer;
    private float shotTimer;
    private float endTimer;
    private int shotCount;
    private bool started;
    private bool finishedFire;

    public override void Enter(GameObject Owner, SC_EnemyStatusManager Manager)
    {
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

        SC_ObjectPool pool = boss.GetStraightMissilePool();
        if (pool == null) return;

        float rotateOffset =
            rotateOffsetPerShot * currentShotIndex;

        if (angleSectorList == null || angleSectorList.Length == 0)
        {
            FireSector(
                Owner,
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
                pool,
                angle
            );
        }
    }

    private void FireOneMissile(
        GameObject Owner,
        SC_ObjectPool pool,
        float angle)
    {
        Vector3 dir =
            Quaternion.Euler(0f, angle, 0f) *
            Owner.transform.forward;

        dir.y = 0f;

        if (dir.sqrMagnitude <= 0.0001f)
        {
            dir = Owner.transform.forward;
        }

        dir.Normalize();

        Vector3 spawnPos = Owner.transform.position + dir * spawnDistance;
        spawnPos.y = 0.0f;
        spawnPos += Vector3.up * spawnHeight;


        GameObject missileObj = pool.GetObject(
            spawnPos,
            Quaternion.LookRotation(dir)
        );

        if (missileObj == null) return;

        SC_StraightMissile missile =
            missileObj.GetComponent<SC_StraightMissile>();

        if (missile != null)
        {
            missile.SetPool(pool);
            missile.OnGetFromPool();

            missile.Init(
                dir,
                missileSpeed,
                0f
            );
        }
    }


    private void ShowWarning(GameObject Owner)
    {
        if (HasUsableSectorList() && showSectorWarningWhenListExists)
        {
            ShowWarningSectors(Owner);
            return;
        }

        ShowWarningCircle(Owner);
    }

    private void ShowWarningCircle(GameObject Owner)
    {
        if (!showCircleWarning) return;

        SC_BossAttackController boss = Owner.GetComponent<SC_BossAttackController>();
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

    private void ShowWarningSectors(GameObject Owner)
    {
        SC_BossAttackController boss =
            Owner.GetComponent<SC_BossAttackController>();

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
                pool,
                sector.GetCenterAngle(),
                sector.GetAngleRange()
            );
        }
    }

    private void ShowOneWarningSector(
    GameObject Owner,
    SC_ObjectPool pool,
    float centerAngle,
    float angleRange
)
    {
        Vector3 spawnPos = Owner.transform.position;
        spawnPos.y = 0.0f;
        spawnPos += Vector3.up * warningHeightOffset;

        Quaternion rotation =
            Quaternion.LookRotation(Owner.transform.forward);

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
}