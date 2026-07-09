using UnityEngine;

public class SC_StraightMissile : MonoBehaviour, SC_IPoolObject
{
    [Tooltip("生存時間"), SerializeField]
    private float lifeTime = 5.0f;

    [Header("Damage")]
    [Tooltip("プレイヤーに与えるダメージ"), SerializeField]
    private int playerDamage = 1;

    private SC_ObjectPool ownerPool;

    private Vector3 moveDirection;
    private float speed;
    private float timer;
    private bool initialized;

    private bool stopYVelocityNearGround;
    private float groundStopY;

    public void SetPool(SC_ObjectPool pool)
    {
        ownerPool = pool;
    }

    public void OnGetFromPool()
    {
        timer = 0f;
        initialized = false;
    }

    public void Init(Vector3 direction, float speed, float startDelay = 0f)
    {
        moveDirection = direction.normalized;
        this.speed = speed;

        timer = -startDelay;
        initialized = true;

        if (moveDirection.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }

    public void SetStopYVelocityNearGround(bool enable,float groundY)
    {
        stopYVelocityNearGround = enable;
        groundStopY = groundY;
    }

    private void Update()
    {
        if (!initialized) return;

        timer += Time.deltaTime;

        if (timer < 0f)
        {
            return;
        }

        if (stopYVelocityNearGround && transform.position.y <= groundStopY)
        {
            moveDirection.y = 0f;

            if (moveDirection.sqrMagnitude > 0.0001f)
            {
                moveDirection.Normalize();
            }
        }

        transform.position += moveDirection * speed * Time.deltaTime;

        if (timer >= lifeTime)
        {
            ReturnToPool();
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

    public void ReturnToPool()
    {
        initialized = false;

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