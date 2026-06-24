using UnityEngine;

public class SC_Door : MonoBehaviour
{
    [Header("Blow Away")]
    [Tooltip("吹っ飛び速度")]
    [SerializeField] private float blowPower = 10.0f;

    [Tooltip("吹っ飛んだ後、何秒後から衝突爆発を有効にするか")]
    [SerializeField] private float collisionEnableDelay = 0.1f;

    [Header("Explosion")]
    [Tooltip("爆発エフェクトのキー")]
    [SerializeField] private string explosionEffectKey = "Explosion";

    [Tooltip("この速度以下になったら爆発")]
    [SerializeField] private float explodeSpeedThreshold = 0.5f;

    [Tooltip("吹っ飛び開始直後にすぐ爆発しないための猶予時間")]
    [SerializeField] private float minExplodeTime = 0.2f;

    [Header("Hit Enemy")]
    [Tooltip("Enemyに当たった時の吹っ飛ばし力")]
    [SerializeField] private float enemyBlowPower = 8.0f;

    [Header("Hit Check")]
    [SerializeField] private float hitRadius = 0.5f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Enemy Aim Assist")]
    [Tooltip("進行方向から何度以内のEnemyを狙うか")]
    [SerializeField] private float enemySearchAngle = 30.0f;

    [Tooltip("Enemyを探す距離")]
    [SerializeField] private float enemySearchDistance = 20.0f;

    [Tooltip("Enemyを狙うか")]
    [SerializeField] private bool useEnemyAimAssist = true;

    private Rigidbody rb;

    private bool isBlownAway;
    private Vector3 previousPosition;
    private float timer;
    private bool canExplode;
    private bool hasHitEnemy;
    private bool hasExploded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        timer = 0;
        canExplode = false;
    }

    private void Update()
    {
        if (!isBlownAway) return;
        if (hasExploded) return;

        timer += Time.deltaTime;

        if (timer >= collisionEnableDelay)
        {
            canExplode = true;
        }

        CheckExplodeBySpeed();
    }

    private void FixedUpdate()
    {
        if (!isBlownAway) return;
        if (hasHitEnemy) return;

        CheckEnemyHitByOverlap();

        if (hasHitEnemy) return;

        CheckEnemyHitBySphereCast();

        previousPosition = transform.position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isBlownAway) return;
        if (hasExploded) return;

        SC_EnemyStatusManager enemy =
            collision.gameObject.GetComponent<SC_EnemyStatusManager>();

        if (enemy == null)
        {
            enemy = collision.gameObject.GetComponentInParent<SC_EnemyStatusManager>();
        }

        if (enemy != null)
        {
            HitEnemy(enemy);
            return;
        }


        if (collision.gameObject.CompareTag("Wall")) 
        {
            Explode();
            return;
        }
    }

    public void BlowAwayByPlayer(Transform player)
    {
        if (isBlownAway) return;
        if (player == null) return;

        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        if (rb == null) return;

        rb.isKinematic = false;
        rb.useGravity = true;

        Vector3 dir = transform.position - player.position;
        dir.y = 0f;

        if (dir.sqrMagnitude <= 0.0001f)
        {
            dir = player.forward;
            dir.y = 0f;
        }

        dir.Normalize();

        dir = SearchEnemyInMoveDirection(dir);

        Vector3 velocity = dir * blowPower;

        rb.linearVelocity = velocity;

        previousPosition = transform.position;
        isBlownAway = true;
        canExplode = false;
        timer = 0f;
        hasHitEnemy = false;
        hasExploded = false;
    }

    private void CheckEnemyHitBySphereCast()
    {
        Vector3 currentPosition = transform.position;
        Vector3 move = currentPosition - previousPosition;

        float distance = move.magnitude;

        if (distance <= 0.0001f)
        {
            return;
        }

        RaycastHit hit;

        bool isHit = Physics.SphereCast(
            previousPosition,
            hitRadius,
            move.normalized,
            out hit,
            distance,
            enemyLayer,
            QueryTriggerInteraction.Collide
        );

        if (!isHit)
        {
            return;
        }

        SC_EnemyStatusManager enemy =
            hit.collider.GetComponent<SC_EnemyStatusManager>();

        if (enemy == null)
        {
            enemy = hit.collider.GetComponentInParent<SC_EnemyStatusManager>();
        }

        if (enemy == null)
        {
            return;
        }

        HitEnemy(enemy);
    }

    private void HitEnemy(SC_EnemyStatusManager enemy)
    {
        if (enemy == null) return;
        if (hasHitEnemy) return;

        hasHitEnemy = true;

        Debug.Log("Door hit enemy : " + enemy.name);

        SC_EnemyStartGate.OpenGate();

        try
        {
            enemy.ForceBlowAway(
                enemyBlowPower,
                transform.position,
                AttackType.Door,
                true
            );
        }
        catch (System.Exception e)
        {
            Debug.LogError("ForceBlowAway error : " + e);
        }
        finally
        {
            Explode();
        }
    }

    private Vector3 SearchEnemyInMoveDirection(Vector3 moveDirection)
    {
        if (!useEnemyAimAssist)
        {
            return moveDirection;
        }

        if (moveDirection.sqrMagnitude <= 0.0001f)
        {
            return moveDirection;
        }

        moveDirection.y = 0f;
        moveDirection.Normalize();

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        GameObject targetEnemy = null;
        float closestDistance = Mathf.Infinity;

        for (int i = 0; i < enemies.Length; i++)
        {
            GameObject enemy = enemies[i];

            if (enemy == null) continue;

            Vector3 toEnemy = enemy.transform.position - transform.position;
            toEnemy.y = 0f;

            float distance = toEnemy.magnitude;

            if (distance <= 0.0001f) continue;
            if (distance > enemySearchDistance) continue;

            Vector3 toEnemyDir = toEnemy.normalized;

            float angle = Vector3.Angle(moveDirection, toEnemyDir);

            if (angle <= enemySearchAngle)
            {
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    targetEnemy = enemy;
                }
            }
        }

        if (targetEnemy != null)
        {
            Vector3 targetDir = targetEnemy.transform.position - transform.position;
            targetDir.y = 0f;

            if (targetDir.sqrMagnitude > 0.0001f)
            {
                return targetDir.normalized;
            }
        }

        return moveDirection;
    }

    private void CheckEnemyHitByOverlap()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            hitRadius,
            enemyLayer,
            QueryTriggerInteraction.Collide
        );

        for (int i = 0; i < hits.Length; i++)
        {
            SC_EnemyStatusManager enemy =
                hits[i].GetComponent<SC_EnemyStatusManager>();

            if (enemy == null)
            {
                enemy = hits[i].GetComponentInParent<SC_EnemyStatusManager>();
            }

            if (enemy != null)
            {
                HitEnemy(enemy);
                return;
            }
        }
    }

    private void CheckExplodeBySpeed()
    {
        if (rb == null) return;

        // 吹っ飛ばした直後は速度判定しない
        if (timer < minExplodeTime) return;

        // まだ爆発可能時間になっていないなら爆発しない
        if (!canExplode) return;

        float speed = rb.linearVelocity.magnitude;

        if (speed <= explodeSpeedThreshold)
        {
            Debug.Log("Door speed low, explode. Speed : " + speed);
            Explode();
        }
    }

    private void Explode()
    {
        if (hasExploded) return;

        hasExploded = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (SC_EffectManager.Instance != null)
        {
            SC_EffectManager.Instance.PlayEffect(
                explosionEffectKey,
                transform.position
            );
        }

        Destroy(gameObject);
    }
}
