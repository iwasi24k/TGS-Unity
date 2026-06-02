using UnityEngine;

public class SC_BulletMulti : MonoBehaviour, SC_IPoolObject
{
    [Tooltip("生存時間"), SerializeField]
    private float lifeTime = 3f;

    [Tooltip("拡散移動の時間"), SerializeField]
    private float spreadMoveTime = 0.3f;

    [Tooltip("拡散移動の速度"), SerializeField]
    private float spreadSpeed = 10f;

    [Tooltip("直進移動の速度"), SerializeField]
    private float straightSpeed = 15f;

    private SC_ObjectPool ownerPool;

    private float timer;
    private Vector3 initialDirection;
    private Vector3 straightDirection;
    private bool straightDirectionSet;

    private Transform player;
    private Transform owner;

    private bool hasHit;
    private bool initialized;

    public void SetPool(SC_ObjectPool pool)
    {
        ownerPool = pool;
    }

    public void OnGetFromPool()
    {
        timer = 0f;

        initialDirection = transform.forward;
        straightDirection = transform.forward;
        straightDirectionSet = false;

        player = null;
        owner = null;

        hasHit = false;
        initialized = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void Init(Transform ownerTransform, Transform playerTransform)
    {
        owner = ownerTransform;
        player = playerTransform;

        timer = 0f;
        hasHit = false;
        straightDirectionSet = false;
        initialized = true;

        // Poolから出した時点の向きを初期拡散方向にする
        initialDirection = transform.forward;
        straightDirection = transform.forward;
    }

    private void Update()
    {
        if (!initialized) return;

        timer += Time.deltaTime;

        if (timer >= lifeTime)
        {
            ReturnToPool();
            return;
        }

        if (timer < spreadMoveTime)
        {
            transform.position +=
                initialDirection *
                spreadSpeed *
                Time.deltaTime;

            return;
        }

        if (!straightDirectionSet)
        {
            SetStraightDirection();
        }

        transform.position +=
            straightDirection *
            straightSpeed *
            Time.deltaTime;
    }

    private void SetStraightDirection()
    {
        if (owner != null && player != null)
        {
            straightDirection =
                player.position -
                owner.position;

            if (straightDirection.sqrMagnitude <= 0.0001f)
            {
                straightDirection = transform.forward;
            }
        }
        else
        {
            straightDirection = transform.forward;
        }

        straightDirection.Normalize();
        straightDirectionSet = true;

        if (straightDirection.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(straightDirection);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;

        if (ShouldIgnore(collision.gameObject))
        {
            return;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            // TODO: プレイヤー体力を減らす処理
        }

        hasHit = true;
        ReturnToPool();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        if (ShouldIgnore(other.gameObject))
        {
            return;
        }

        if (other.gameObject.CompareTag("Player"))
        {
            // TODO: プレイヤー体力を減らす処理
        }

        hasHit = true;
        ReturnToPool();
    }

    private bool ShouldIgnore(GameObject obj)
    {
        if (obj.CompareTag("Enemy")) return true;
        if (obj.CompareTag("Bullet")) return true;
        if (obj.CompareTag("Field")) return true;

        return false;
    }

    public void ReturnToPool()
    {
        initialized = false;
        hasHit = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

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