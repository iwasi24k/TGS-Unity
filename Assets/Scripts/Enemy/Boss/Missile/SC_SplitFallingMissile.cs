using UnityEngine;

public class SC_SplitFallingMissile : MonoBehaviour, SC_IPoolObject
{
    [Header("Parent Warning")]
    [Tooltip("親ミサイル落下地点の円形Warning半径"), SerializeField]
    private float parentWarningRadius = 1.5f;

    [Tooltip("親Warningを地面から少し浮かせる高さ"), SerializeField]
    private float parentWarningHeightOffset = 0.04f;

    [Header("Child Warning")]
    [Tooltip("子ミサイル発射方向に四角形Warningを出すか"), SerializeField]
    private bool showChildWarning = true;

    [Tooltip("子ミサイルWarningの幅"), SerializeField]
    private float childWarningWidth = 1.0f;

    [Tooltip("子ミサイルWarningの長さ"), SerializeField]
    private float childWarningLength = 10.0f;

    [Tooltip("子ミサイルWarningを地面から少し浮かせる高さ"), SerializeField]
    private float childWarningHeightOffset = 0.01f;

    [Header("Child Missile")]
    [Tooltip("子ミサイルを地面から少し上に出す高さ"), SerializeField]
    private float childMissileSpawnHeight = 1.5f;

    [Header("Damage")]
    [Tooltip("プレイヤーに与えるダメージ"), SerializeField]
    private int playerDamage = 1;

    private SC_ObjectPool ownerPool;
    private SC_ObjectPool warningPool;
    private SC_ObjectPool childMissilePool;
    private SC_ObjectPool childWarningPool;

    private Vector3 targetPosition;
    private Vector3 startPosition;

    private float fallTime;
    private float timer;

    private GameObject warningMark;
    private int childMissileCount;
    private float childMissileSpeed;

    private bool initialized;

    public void SetPool(SC_ObjectPool pool)
    {
        ownerPool = pool;
    }

    public void SetWarningPool(SC_ObjectPool pool)
    {
        warningPool = pool;
    }

    public void SetChildMissilePool(SC_ObjectPool pool)
    {
        childMissilePool = pool;
    }

    public void SetChildWarningPool(SC_ObjectPool pool)
    {
        childWarningPool = pool;
    }

    public void OnGetFromPool()
    {
        timer = 0f;
        fallTime = 0f;

        targetPosition = Vector3.zero;
        startPosition = Vector3.zero;

        warningMark = null;

        childMissileCount = 0;
        childMissileSpeed = 0f;

        initialized = false;
    }

    public void Init(
        Vector3 targetPosition,
        float height,
        float fallTime,
        int childMissileCount,
        float childMissileSpeed
    )
    {
        this.targetPosition = targetPosition;
        this.fallTime = Mathf.Max(0.01f, fallTime);
        this.childMissileCount = childMissileCount;
        this.childMissileSpeed = childMissileSpeed;

        timer = 0f;
        initialized = true;

        startPosition = targetPosition + Vector3.up * height;
        transform.position = startPosition;

        // 親ミサイルの落下地点Warning
        CreateParentWarning();

        // 子ミサイルの直線Warningも最初から出す
        CreateChildMissileWarnings();
    }

    private void Update()
    {
        if (!initialized) return;

        timer += Time.deltaTime;

        float t = timer / fallTime;
        t = Mathf.Clamp01(t);

        transform.position = Vector3.Lerp(
            startPosition,
            targetPosition,
            t
        );

        if (t >= 1.0f)
        {
            ExplodeAndSplit();
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            SC_PlayerHP playerHP = other.GetComponent<SC_PlayerHP>();

            if (playerHP == null)
            {
                playerHP = other.GetComponentInParent<SC_PlayerHP>();
            }

            if (playerHP != null)
            {
                playerHP.TakeDamage(playerDamage, transform.position);
            }

            ExplodeAndSplit();
            return;
        }

        if (other.CompareTag("Wall"))
        {
            ReturnToPool();
            return;
        }
    }

    private void CreateParentWarning()
    {
        if (warningPool == null) return;

        GameObject warningObj = warningPool.GetObject(
            targetPosition + Vector3.up * parentWarningHeightOffset,
            Quaternion.identity
        );

        if (warningObj == null) return;

        warningMark = warningObj;

        SC_WarningTelegraphCircle warningCircle =
            warningObj.GetComponent<SC_WarningTelegraphCircle>();

        if (warningCircle != null)
        {
            warningCircle.SetPool(warningPool);
            warningCircle.OnGetFromPool();

            // fallTime秒でゲージMAX。
            // つまり親ミサイル着弾と完全同期。
            warningCircle.Init(
                parentWarningRadius,
                fallTime
            );
        }
    }

    private void CreateChildMissileWarnings()
    {
        if (!showChildWarning) return;
        if (childWarningPool == null) return;
        if (childMissileCount <= 0) return;

        for (int i = 0; i < childMissileCount; i++)
        {
            Vector3 dir = GetChildMissileDirection(i);
            CreateOneChildWarning(dir);
        }
    }

    private Vector3 GetChildMissileDirection(int index)
    {
        float angle = 360f / childMissileCount * index;

        Vector3 dir =
            Quaternion.Euler(0f, angle, 0f) *
            Vector3.forward;

        dir.y = 0f;

        if (dir.sqrMagnitude <= 0.0001f)
        {
            dir = Vector3.forward;
        }

        return dir.normalized;
    }

    private void CreateOneChildWarning(Vector3 direction)
    {
        Vector3 spawnPos =
            targetPosition +
            Vector3.up * childWarningHeightOffset;

        GameObject warningObj = childWarningPool.GetObject(
            spawnPos,
            Quaternion.LookRotation(direction)
        );

        if (warningObj == null) return;

        SC_WarningTelegraphRectangle warning =
            warningObj.GetComponent<SC_WarningTelegraphRectangle>();

        if (warning != null)
        {
            warning.SetPool(childWarningPool);
            warning.OnGetFromPool();

            // ここが重要。
            // 子WarningもfallTime秒でゲージMAX。
            // 親ミサイル着弾と同時に子ミサイル発射。
            warning.Init(
                childWarningWidth,
                childWarningLength,
                fallTime
            );
        }
    }

    private void ExplodeAndSplit()
    {
        ReturnParentWarning();

        // 子WarningはfallTimeで自動的にPoolへ戻る想定。
        // 着弾と同時に子ミサイルを発射する。
        SpawnChildMissiles();

        ReturnToPool();
    }

    private void SpawnChildMissiles()
    {
        if (childMissilePool == null) return;
        if (childMissileCount <= 0) return;

        for (int i = 0; i < childMissileCount; i++)
        {
            Vector3 dir = GetChildMissileDirection(i);

            Vector3 spawnPos =
                targetPosition +
                Vector3.up * childMissileSpawnHeight;

            GameObject missileObj = childMissilePool.GetObject(
                spawnPos,
                Quaternion.LookRotation(dir)
            );

            if (missileObj == null) continue;

            SC_StraightMissile missile =
                missileObj.GetComponent<SC_StraightMissile>();

            if (missile != null)
            {
                missile.SetPool(childMissilePool);
                missile.OnGetFromPool();

                missile.Init(
                    dir,
                    childMissileSpeed,
                    0f
                );
            }
        }
    }

    private void ReturnParentWarning()
    {
        if (warningMark == null) return;

        SC_IPoolObject poolObject =
            warningMark.GetComponent<SC_IPoolObject>();

        if (poolObject != null)
        {
            poolObject.ReturnToPool();
        }
        else if (warningPool != null)
        {
            warningPool.ReturnObject(warningMark);
        }
        else
        {
            Destroy(warningMark);
        }

        warningMark = null;
    }

    public void ReturnToPool()
    {
        SC_EffectManager.Instance.PlayEffect("SmallExplosion", this.transform.position);
        SC_SEManager.Instance.PlaySE("Explo_N2", this.transform.position);

        initialized = false;

        ReturnParentWarning();

        if (ownerPool != null)
        {
            ownerPool.ReturnObject(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
