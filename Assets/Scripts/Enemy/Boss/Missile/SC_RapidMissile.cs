using UnityEngine;

public class SC_RapidMissile : MonoBehaviour, SC_IPoolObject
{
    [Tooltip("生存時間"), SerializeField]
    private float lifeTime = 5.0f;

    [Header("Damage")]
    [Tooltip("プレイヤーに与えるダメージ"), SerializeField]
    private int playerDamage = 1;

    private SC_ObjectPool ownerPool;

    private Transform target;
    private float speed;
    private float startDelay;

    private float timer;
    private bool initialized;
    private Vector3 moveDirection;

    private enum MissileState
    {
        CurveSpread,
        Launch
    }

    private MissileState missileState;

    private float curveTime;
    private Vector3 curveStartPos;
    private Vector3 curveControlPos;
    private Vector3 curveEndPos;
    private Vector3 spreadOffset;
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

        missileState = MissileState.CurveSpread;

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
    float curveTime,
    Vector3 curveControlOffset,
    Vector3 curveEndOffset,
    float rotateSpeed)
    {
        this.target = target;
        this.speed = speed;
        this.startDelay = Mathf.Max(0f, startDelay);

        this.curveTime = Mathf.Max(0.01f, curveTime);
        this.rotateSpeed = rotateSpeed;

        timer = 0f;
        stateTimer = 0f;
        initialized = true;

        missileState = MissileState.CurveSpread;

        curveStartPos = transform.position;
        curveControlPos = curveStartPos + curveControlOffset;
        curveEndPos = curveStartPos + curveEndOffset;

        moveDirection = Vector3.up;

        transform.rotation = Quaternion.LookRotation(Vector3.up);
    }

    private void Update()
    {
        if (!initialized) return;

        timer += Time.deltaTime;
        stateTimer += Time.deltaTime;

        switch (missileState)
        {
            case MissileState.CurveSpread:
                UpdateCurveSpread();
                break;

            case MissileState.Launch:
                UpdateLaunch();
                break;
        }

        if (timer >= lifeTime)
        {
            ReturnToPool();
        }
    }

    private void UpdateCurveSpread()
    {
        float t = stateTimer / curveTime;
        t = Mathf.Clamp01(t);

        float smoothT = Mathf.SmoothStep(0f, 1f, t);

        Vector3 oldPos = transform.position;

        transform.position = QuadraticBezier(
            curveStartPos,
            curveControlPos,
            curveEndPos,
            smoothT
        );

        Vector3 dir = transform.position - oldPos;

        if (dir.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(dir.normalized);
        }

        transform.Rotate(
            Vector3.forward,
            rotateSpeed * Time.deltaTime,
            Space.Self
        );

        if (t >= 1.0f)
        {
            // 曲線展開が終わった瞬間にPlayer方向を決める
            Launch();

            // ロックオンマークを消す
            ReturnWarningMark();

            missileState = MissileState.Launch;
            stateTimer = 0f;
        }
    }

    private Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1.0f - t;

        return
            u * u * p0 +
            2.0f * u * t * p1 +
            t * t * p2;
    }

    private void UpdateLaunch()
    {
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    private void Launch()
    {
        if (target != null)
        {
            // 発射瞬間のPlayer位置を見る
            // Y方向も含めるので、高い位置からでもPlayerへ向かう
            Vector3 dir = target.position - transform.position;

            if (dir.sqrMagnitude <= 0.0001f)
            {
                dir = transform.forward;
            }

            moveDirection = dir.normalized;
        }
        else
        {
            moveDirection = transform.forward;
        }

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
