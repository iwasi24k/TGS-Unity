using UnityEngine;

public class SC_HomingMissile : MonoBehaviour, SC_IPoolObject
{
    [Tooltip("生成後、動き始めるまでの待機時間"), SerializeField]
    private float startDelay = 0.5f;

    [Tooltip("生存時間"), SerializeField]
    private float lifeTime = 5.0f;

    [Header("Damage")]
    [Tooltip("プレイヤーに与えるダメージ"), SerializeField]
    private float playerDamage = 1.0f;

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

    public void SetPool(SC_ObjectPool pool)
    {
        ownerPool = pool;
    }

    public void OnGetFromPool()
    {
        target = null;
        speed = 0f;
        homingTime = 0f;

        timer = 0f;
        delayTimer = 0f;
        lifeTimer = 0f;

        moveDirection = transform.forward;
        isStarted = false;
        initialized = false;
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
