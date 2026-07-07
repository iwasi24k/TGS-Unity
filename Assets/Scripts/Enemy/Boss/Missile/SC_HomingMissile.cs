using UnityEngine;

public class SC_HomingMissile : MonoBehaviour, SC_IPoolObject
{
    [Tooltip("空中展開後、追尾開始まで止まる時間"), SerializeField]
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
    private float stateTimer;
    private float lifeTimer;

    private Vector3 moveDirection;
    private bool initialized;

    private enum MissileState
    {
        CurveSpread,
        Wait,
        Homing,
        Straight
    }

    private MissileState missileState;

    [Header("Curve Spread")]
    [Tooltip("傘の骨のように広がる時間")]
    [SerializeField] private float curveTime = 0.8f;

    [Tooltip("曲線中の回転速度")]
    [SerializeField] private float rotateSpeed = 720.0f;

    private Vector3 curveStartPos;
    private Vector3 curveControlPos;
    private Vector3 curveEndPos;

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

    public void SetUseLockOnMark(bool use)
    {
        useThisLockOnMark = use;
    }

    public void OnGetFromPool()
    {
        ReturnWarningMark();

        target = null;
        speed = 0f;
        homingTime = 0f;

        timer = 0f;
        stateTimer = 0f;
        lifeTimer = 0f;

        moveDirection = transform.forward;
        initialized = false;

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
        float homingTime,
        float curveTime,
        Vector3 curveControlOffset,
        Vector3 curveEndOffset,
        float rotateSpeed
    )
    {
        this.target = target;
        this.speed = speed;
        this.homingTime = Mathf.Max(0f, homingTime);

        this.curveTime = Mathf.Max(0.01f, curveTime);
        this.rotateSpeed = rotateSpeed;

        timer = 0f;
        stateTimer = 0f;
        lifeTimer = 0f;

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

        lifeTimer += Time.deltaTime;

        if (lifeTimer >= lifeTime)
        {
            ReturnToPool();
            return;
        }

        stateTimer += Time.deltaTime;

        switch (missileState)
        {
            case MissileState.CurveSpread:
                UpdateCurveSpread();
                break;

            case MissileState.Wait:
                UpdateWait();
                break;

            case MissileState.Homing:
                UpdateHoming();
                break;

            case MissileState.Straight:
                UpdateStraight();
                break;
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
            missileState = MissileState.Wait;
            stateTimer = 0f;

            // 空中に展開してからロックオン表示
            CreateLockOnMark(startDelay);
        }
    }

    private void UpdateWait()
    {
        // 空中で止まって回転だけする
        transform.Rotate(
            Vector3.forward,
            rotateSpeed * Time.deltaTime,
            Space.Self
        );

        if (stateTimer >= startDelay)
        {
            ReturnWarningMark();

            missileState = MissileState.Homing;
            stateTimer = 0f;
            timer = 0f;

            SetDirectionToPlayer();
        }
    }

    private void UpdateHoming()
    {
        timer += Time.deltaTime;

        if (timer <= homingTime && target != null)
        {
            SetDirectionToPlayer();
        }
        else
        {
            missileState = MissileState.Straight;
            stateTimer = 0f;
        }

        MoveForward();
    }

    private void UpdateStraight()
    {
        MoveForward();
    }

    private void MoveForward()
    {
        transform.position += moveDirection * speed * Time.deltaTime;

        if (moveDirection.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }

    private void SetDirectionToPlayer()
    {
        if (target != null)
        {
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
    }

    private Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1.0f - t;

        return
            u * u * p0 +
            2.0f * u * t * p1 +
            t * t * p2;
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

    private void CreateLockOnMark(float duration)
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
            duration
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
