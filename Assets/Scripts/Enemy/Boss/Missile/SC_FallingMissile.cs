using UnityEngine;

public class SC_FallingMissile : MonoBehaviour, SC_IPoolObject
{
    [Tooltip("落下地点警告の半径"), SerializeField]
    private float warningRadius = 1.5f;

    [Header("Damage")]
    [Tooltip("プレイヤーに与えるダメージ"), SerializeField]
    private float playerDamage = 1.0f;

    private SC_ObjectPool ownerPool;
    private SC_ObjectPool warningPool;

    private Vector3 targetPosition;
    private Vector3 startPosition;

    private float fallTime;
    private float timer;

    private GameObject warningMark;
    private bool initialized;

    public void SetPool(SC_ObjectPool pool)
    {
        ownerPool = pool;
    }

    public void SetWarningPool(SC_ObjectPool pool)
    {
        warningPool = pool;
    }

    public void OnGetFromPool()
    {
        timer = 0f;
        fallTime = 0f;

        targetPosition = Vector3.zero;
        startPosition = Vector3.zero;

        warningMark = null;
        initialized = false;
    }

    public void Init(
        Vector3 targetPosition,
        float height,
        float fallTime
    )
    {
        this.targetPosition = targetPosition;
        this.fallTime = Mathf.Max(0.01f, fallTime);

        timer = 0f;
        initialized = true;

        startPosition = targetPosition + Vector3.up * height;
        transform.position = startPosition;

        CreateWarningMark();
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
            Explode();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        SC_PlayerHP playerHP = other.GetComponent<SC_PlayerHP>();

        // Player本体ではなく子Colliderに当たった場合用
        if (playerHP == null)
        {
            playerHP = other.GetComponentInParent<SC_PlayerHP>();
        }

        if (playerHP != null)
        {
            playerHP.TakeDamage((int)playerDamage);
        }

        if (other.CompareTag("Player"))
        {
            ReturnToPool();
            return;
        }

        if (other.CompareTag("Wall"))
        {
            ReturnToPool();
            return;
        }
    }

    private void CreateWarningMark()
    {
        if (warningPool == null) return;

        GameObject warningObj = warningPool.GetObject(
            targetPosition + Vector3.up * 0.03f,
            Quaternion.identity
        );

        if (warningObj == null) return;

        warningMark = warningObj;

        SC_WarningTelegraphCircle warning =
            warningObj.GetComponent<SC_WarningTelegraphCircle>();

        if (warning != null)
        {
            warning.SetPool(warningPool);
            warning.OnGetFromPool();

            warning.Init(
                warningRadius,
                fallTime
            );
        }
    }

    private void Explode()
    {
        ReturnWarningMark();

        // 必要ならここでPlayerへのダメージ・ノックバック
        Collider[] hits = Physics.OverlapSphere(targetPosition, warningRadius);

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if (rb != null && !rb.isKinematic)
            {
                Vector3 dir = hit.transform.position - targetPosition;
                dir.y = 0f;

                if (dir.sqrMagnitude > 0.0001f)
                {
                    dir.Normalize();
                    rb.AddForce(dir * 10.0f, ForceMode.Impulse);
                }
            }
        }

        ReturnToPool();
    }

    private void ReturnWarningMark()
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
        initialized = false;

        ReturnWarningMark();

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
