using UnityEngine;
using UnityEngine.iOS;

public class SC_HomingMissile : MonoBehaviour, SC_IPoolObject
{
    [Tooltip("生成後、動き始めるまでの待機時間"), SerializeField]
    private float startDelay = 0.5f;

    [Tooltip("生存時間"), SerializeField]
    private float lifeTime = 5.0f;

    [Header("Damage")]
    [Tooltip("プレイヤーに与えるダメージ"), SerializeField]
    private int playerDamage = 1;

    private SC_ObjectPool ownerPool;

    private Transform target;
    private float speed;
    private float homingTime;

    private float timer;
    private float delayTimer;
    private float lifeTimer;

    private Vector3 moveDirection;
    private bool isStarted;
    private bool initialized;

    [Header("Lock On Mark")]
    [Tooltip("ロックオンマークを表示するか")]
    [SerializeField] private bool useLockOnMark = true;

    [Tooltip("ロックオンマークの半径")]
    [SerializeField] private float lockOnRadius = 1.2f;

    [Tooltip("地面から少し浮かせる高さ")]
    [SerializeField] private float lockOnGroundOffset = 0.05f;

    private SC_ObjectPool warningPool;
    private GameObject warningMarkObj;
    private SC_WarningTelegraphCircle warningMark;
    private bool useThisLockOnMark = true;

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
        ReturnWarningMark();

        target = null;
        speed = 0f;
        homingTime = 0f;

        timer = 0f;
        delayTimer = 0f;
        lifeTimer = 0f;

        moveDirection = transform.forward;
        isStarted = false;
        initialized = false;

        useThisLockOnMark = true;
        warningMarkObj = null;
        warningMark = null;
    }

    public void Init(Transform target, float speed, float homingTime)
    {
        this.target = target;
        this.speed = speed;
        this.homingTime = homingTime;

        timer = 0f;
        delayTimer = 0f;
        lifeTimer = 0f;
        isStarted = false;
        initialized = true;

        if (target != null)
        {
            Vector3 dir = target.position - transform.position;

            if (dir.sqrMagnitude <= 0.0001f)
            {
                dir = transform.forward;
            }

            moveDirection = dir.normalized;
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
        else
        {
            moveDirection = transform.forward;
        }

        CreateLockOnMark();
    }


    private void Update()
    {
        if (!initialized) return;

        lifeTimer += Time.deltaTime;

        if (lifeTimer >= lifeTime)
        {
            ReturnToPool();
            return;
        }

        if (!isStarted)
        {
            delayTimer += Time.deltaTime;

            if (delayTimer < startDelay)
            {
                return;
            }

            isStarted = true;

            ReturnWarningMark();
        }

        timer += Time.deltaTime;

        if (timer <= homingTime && target != null)
        {
            Vector3 dir = target.position - transform.position;

            if (dir.sqrMagnitude > 0.0001f)
            {
                moveDirection = dir.normalized;
            }
        }

        transform.position += moveDirection * speed * Time.deltaTime;

        if (moveDirection.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SC_PlayerHP playerHP = other.GetComponent<SC_PlayerHP>();

            // Player本体ではなく子Colliderに当たった場合用
            if (playerHP == null)
            {
                playerHP = other.GetComponentInParent<SC_PlayerHP>();
            }

            if (playerHP != null)
            {
                playerHP.TakeDamage(playerDamage);
            }

            ReturnToPool();
            return;
        }

        if (other.CompareTag("Wall"))
        {
            ReturnToPool();
            return;
        }
    }

    private void CreateLockOnMark()
    {
        if (!useThisLockOnMark) return;
        if (!useLockOnMark) return;
        if (warningPool == null) return;
        if (target == null) return;

        Vector3 markPos = target.position;
        markPos.y = lockOnGroundOffset;

        warningMarkObj = warningPool.GetObject(
            markPos,
            Quaternion.identity
        );

        if (warningMarkObj == null) return;

        warningMark = warningMarkObj.GetComponent<SC_WarningTelegraphCircle>();

        if (warningMark == null)
        {
            warningMarkObj.SetActive(false);
            warningMarkObj = null;
            return;
        }

        warningMark.SetPool(warningPool);
        warningMark.OnGetFromPool();

        warningMark.Init(
            lockOnRadius,
            startDelay
        );

        warningMark.SetFollowTarget(
            target,
            new Vector3(0f, lockOnGroundOffset, 0f)
        );
    }

    private void ReturnWarningMark()
    {
        if (warningMark != null)
        {
            warningMark.StopFollow();
            warningMark.ReturnToPool();
        }
        else if (warningMarkObj != null)
        {
            warningMarkObj.SetActive(false);
        }

        warningMark = null;
        warningMarkObj = null;
    }

    public void SetUseLockOnMark(bool use)
    {
        useThisLockOnMark = use;
    }
    public void ReturnToPool()
    {
        initialized = false;
        target = null;

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
