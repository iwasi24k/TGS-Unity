using UnityEngine;

public class SC_RapidMissile : MonoBehaviour, SC_IPoolObject
{
    [Tooltip("生存時間"), SerializeField]
    private float lifeTime = 5.0f;

    [Header("Damage")]
    [Tooltip("プレイヤーに与えるダメージ"), SerializeField]
    private int playerDamage = 1;

    [Header("Curve Attack")]
    [Tooltip("曲線でPlayerへ向かう時間")]
    [SerializeField] private float attackCurveTime = 1.0f;

    [Tooltip("最初にどれくらい上へ膨らませるか")]
    [SerializeField] private float curveUpHeight = 5.0f;

    [Tooltip("Playerの少し上を狙う高さ")]
    [SerializeField] private float targetHeightOffset = 1.0f;

    private Vector3 attackStartPos;
    private Vector3 attackControlPos1;
    private Vector3 attackControlPos2;
    private Vector3 attackTargetPos;

    private SC_ObjectPool ownerPool;

    private Transform target;
    private float speed;
    private float startDelay;

    private float timer;
    private bool initialized;
    private Vector3 moveDirection;

    private enum MissileState
    {
        CurveAttack,
        Straight
    }

    private MissileState missileState;

    private float curveTime;
    private Vector3 curveStartPos;
    private Vector3 curveControlPos;
    private Vector3 curveEndPos;

    private float rotateSpeed;
    private float stateTimer;

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
        startDelay = 0f;

        timer = 0f;
        stateTimer = 0f;
        initialized = false;
        moveDirection = transform.forward;

        missileState = MissileState.CurveAttack;

        curveStartPos = Vector3.zero;
        curveControlPos = Vector3.zero;
        curveEndPos = Vector3.zero;

        useThisLockOnMark = true;
        warningMarkObj = null;
        warningMark = null;
    }

    public void Init(
    Transform target,
    float speed,
    float startDelay,
    Vector3 curveSpreadOffset
)
    {
        this.target = target;
        this.speed = speed;
        this.startDelay = Mathf.Max(0f, startDelay);

        timer = 0f;
        stateTimer = 0f;
        initialized = true;

        missileState = MissileState.CurveAttack;

        attackStartPos = transform.position;

        if (target != null)
        {
            attackTargetPos = target.position + Vector3.up * targetHeightOffset;
        }
        else
        {
            attackTargetPos = transform.position + transform.forward * 10f;
        }

        // P1: まず上へ向かう感じを作る
        attackControlPos1 =
            attackStartPos +
            Vector3.up * curveUpHeight;

        // P2: 傘の展開点を制御点にする
        attackControlPos2 =
            attackStartPos +
            Vector3.up * curveUpHeight +
            curveSpreadOffset;

        moveDirection = Vector3.up;

        transform.rotation = Quaternion.LookRotation(Vector3.up);

        CreateLockOnMark();
    }

    private void Update()
    {
        if (!initialized) return;

        timer += Time.deltaTime;
        stateTimer += Time.deltaTime;

        switch (missileState)
        {
            case MissileState.CurveAttack:
                UpdateCurveAttack();
                break;

            case MissileState.Straight:
                UpdateStraight();
                break;
        }

        if (timer >= lifeTime)
        {
            ReturnToPool();
        }
    }

    private void UpdateCurveAttack()
    {
        float t = stateTimer / attackCurveTime;
        t = Mathf.Clamp01(t);

        Vector3 oldPos = transform.position;

        transform.position = CubicBezier(
            attackStartPos,
            attackControlPos1,
            attackControlPos2,
            attackTargetPos,
            t
        );

        Vector3 dir = transform.position - oldPos;

        if (dir.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(dir.normalized);
        }

        if (t >= 1.0f)
        {
            ReturnWarningMark();

            Vector3 straightDir = attackTargetPos - attackControlPos2;

            if (straightDir.sqrMagnitude <= 0.0001f)
            {
                straightDir = transform.forward;
            }

            moveDirection = straightDir.normalized;

            missileState = MissileState.Straight;
            stateTimer = 0f;
        }
    }


    private void UpdateStraight()
    {
        transform.position += moveDirection * speed * Time.deltaTime;

        if (moveDirection.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }

    private Vector3 CubicBezier(
    Vector3 p0,
    Vector3 p1,
    Vector3 p2,
    Vector3 p3,
    float t
)
    {
        float u = 1.0f - t;

        return
            u * u * u * p0 +
            3.0f * u * u * t * p1 +
            3.0f * u * t * t * p2 +
            t * t * t * p3;
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
        ReturnWarningMark();

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
