using UnityEngine;

public class SC_HomingMissile : MonoBehaviour
{
    [Tooltip("生成後、動き始めるまでの待機時間"), SerializeField] private float startDelay = 0.5f;

    private Transform target;
    private float speed;
    private float homingTime;
    private float timer;
    private Vector3 moveDirection;
    private float delayTimer;
    private bool isStarted;

    public void Init(Transform target, float speed, float homingTime)
    {
        this.target = target;
        this.speed = speed;
        this.homingTime = homingTime;

        timer = 0f;
        delayTimer = 0f;
        isStarted = false;

        if (target != null)
        {
            moveDirection = (target.position - transform.position).normalized;
        }
        else
        {
            moveDirection = transform.forward;
        }
    }

    void Update()
    {
        // 生成後、少し空中で停止
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
            moveDirection = (target.position - transform.position).normalized;
        }

        transform.position += moveDirection * speed * Time.deltaTime;

        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 dir = other.transform.position - transform.position;
                dir.y = 0f;
                dir.Normalize();

                rb.AddForce(dir * 8.0f, ForceMode.Impulse);
            }

            Destroy(gameObject);
        }

        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
