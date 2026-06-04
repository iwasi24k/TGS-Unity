using UnityEngine;

public class SC_RapidMissile : MonoBehaviour, SC_IPoolObject
{
    [Tooltip("生存時間"), SerializeField]
    private float lifeTime = 5.0f;

    [Header("Damage")]
    [Tooltip("プレイヤーに与えるダメージ"), SerializeField]
    private float playerDamage = 1.0f;

    private SC_ObjectPool ownerPool;

    private Transform target;
    private float speed;
    private float startDelay;

    private float timer;
    private bool launched;
    private bool initialized;
    private Vector3 moveDirection;

    public void SetPool(SC_ObjectPool pool)
    {
        ownerPool = pool;
    }

    public void OnGetFromPool()
    {
        target = null;
        speed = 0f;
        startDelay = 0f;

        timer = 0f;
        launched = false;
        initialized = false;
        moveDirection = transform.forward;
    }

    public void Init(Transform target, float speed, float startDelay)
    {
        this.target = target;
        this.speed = speed;
        this.startDelay = Mathf.Max(0f, startDelay);

        timer = 0f;
        launched = false;
        initialized = true;
        moveDirection = transform.forward;
    }

    private void Update()
    {
        if (!initialized) return;

        timer += Time.deltaTime;

        if (!launched)
        {
            if (timer < startDelay)
            {
                return;
            }

            // 発射する瞬間のPlayer位置を見る
            Launch();
        }

        transform.position += moveDirection * speed * Time.deltaTime;

        if (timer >= lifeTime)
        {
            ReturnToPool();
        }
    }

    private void Launch()
    {
        launched = true;

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
        launched = false;
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
