using UnityEngine;

public class SC_ReflectableMissile : MonoBehaviour, SC_IPoolObject
{
    [Header("Move")]
    [Tooltip("通常時の速度"), SerializeField]
    private float normalSpeed = 8.0f;

    [Tooltip("反撃状態の速度"), SerializeField]
    private float reflectedSpeed = 15.0f;

    [Tooltip("生存時間"), SerializeField]
    private float lifeTime = 5.0f;

    [Header("Reflect Target")]
    [Tooltip("プレイヤー前方何度以内の敵を探すか"), SerializeField]
    private float searchAngle = 30.0f;

    [Tooltip("反撃時に敵を探す距離"), SerializeField]
    private float searchDistance = 20.0f;

    [Tooltip("敵を探す中心高さ"), SerializeField]
    private float searchHeightOffset = 1.0f;

    [Tooltip("狙う位置の高さ補正"), SerializeField]
    private float targetAimHeight = 1.0f;

    [Header("Damage")]
    [Tooltip("プレイヤーに与えるダメージ"), SerializeField]
    private int playerDamage = 1;

    [Tooltip("反撃後、敵に与えるダメージ"), SerializeField]
    private int enemyDamage = 10;

    [Header("Render")]
    [Tooltip("色を変えるRenderer"), SerializeField]
    private Renderer targetRenderer;

    [Tooltip("通常時の本体色"), SerializeField]
    private Color normalBaseColor = Color.red;

    [Tooltip("通常時の稲妻色"), SerializeField]
    private Color normalLightningColor = Color.yellow;

    [Tooltip("反撃状態の本体色"), SerializeField]
    private Color reflectedBaseColor = Color.blue;

    [Tooltip("反撃状態の稲妻色"), SerializeField]
    private Color reflectedLightningColor = Color.cyan;

    private SC_ObjectPool ownerPool;

    private Vector3 moveDirection;
    private float currentSpeed;
    private float timer;

    private bool initialized;
    private bool reflected;

    private Collider ignoredOwnerCollider;
    private Collider myCollider;

    private MaterialPropertyBlock propertyBlock;

    private bool stopYVelocityNearGround;
    private float groundStopY;
    public void SetPool(SC_ObjectPool pool)
    {
        ownerPool = pool;
    }

    public void OnGetFromPool()
    {
        timer = 0f;
        currentSpeed = normalSpeed;

        moveDirection = transform.forward;

        initialized = false;
        reflected = false;

        ApplyMissileColor(
            normalBaseColor,
            normalLightningColor
        );
    }

    public void Init(Vector3 direction, float speed)
    {
        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction = transform.forward;
        }

        moveDirection = direction.normalized;
        currentSpeed = speed;

        timer = 0f;
        reflected = false;
        initialized = true;

        transform.rotation = Quaternion.LookRotation(moveDirection);

        ApplyMissileColor(
            normalBaseColor,
            normalLightningColor
        );
    }

    public void SetStopYVelocityNearGround(bool enable, float groundY)
    {
        stopYVelocityNearGround = enable;
        groundStopY = groundY;
    }

    private void Update()
    {
        if (!initialized) return;

        timer += Time.deltaTime;

        if (stopYVelocityNearGround && transform.position.y <= groundStopY)
        {
            moveDirection.y = 0f;

            if (moveDirection.sqrMagnitude > 0.0001f)
            {
                moveDirection.Normalize();
            }
        }

        transform.position += moveDirection * currentSpeed * Time.deltaTime;

        if (timer >= lifeTime)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!initialized) return;

        // 通常状態：Playerに当たる
        if (!reflected)
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

            if (other.CompareTag("Enemy"))
            {
                return;
            }
        }

        // 反撃状態：Enemy / Bossに当たる
        else
        {
            SC_EnemyStatusManager enemy =
                other.GetComponentInParent<SC_EnemyStatusManager>();

            Debug.Log("Missile hit: " + other.name);
            if (enemy != null)
            {
                Debug.Log("Missile hit Enemy: " + enemy.name);

                if (enemy.UseBossShield())
                {
                    if (enemy.HasBossShield())
                    {
                        enemy.TakeBossShieldDamage(enemyDamage);
                    }
                    else if (enemy.IsBossDown())
                    {
                        enemy.TakeDamage(
                            enemyDamage,
                            transform.position,
                            false,
                            0,
                            EnemyDamageSource.EnemyCollision
                        );
                    }
                }
                else
                {
                    enemy.TakeDamage(
                        enemyDamage,
                        transform.position,
                        false,
                        0,
                        EnemyDamageSource.EnemyCollision
                    );
                }

                ReturnToPool();
                return;
            }

            if (other.CompareTag("Player"))
            {
                return;
            }
        }


    }

    private void ReflectByPlayerAttack(Collider playerAttackCollider)
    {
        Transform playerTransform = GetPlayerTransform(playerAttackCollider);
        ReflectByPlayer(playerTransform);
    }

    private Transform GetPlayerTransform(Collider playerAttackCollider)
    {
        // PlayerAttackの親にPlayerがいる想定
        Transform current = playerAttackCollider.transform;

        while (current != null)
        {
            if (current.CompareTag("Player"))
            {
                return current;
            }

            current = current.parent;
        }

        GameObject playerObj =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            return playerObj.transform;
        }

        return null;
    }

    private Transform FindEnemyInPlayerFront(Transform playerTransform)
    {
        Vector3 searchCenter =
            playerTransform.position +
            Vector3.up * searchHeightOffset;

        Collider[] hitColliders =
            Physics.OverlapSphere(
                searchCenter,
                searchDistance
            );

        Transform bestTarget = null;
        float bestDistance = float.MaxValue;

        Vector3 playerForward = playerTransform.forward;
        playerForward.y = 0f;

        if (playerForward.sqrMagnitude <= 0.0001f)
        {
            playerForward = Vector3.forward;
        }

        playerForward.Normalize();

        float halfAngle = searchAngle * 0.5f;

        for (int i = 0; i < hitColliders.Length; i++)
        {
            Collider hit = hitColliders[i];

            if (!hit.CompareTag("Enemy"))
            {
                continue;
            }

            // 自分自身のColliderを拾わない
            if (hit.gameObject == gameObject)
            {
                continue;
            }

            Vector3 toTarget =
                hit.transform.position -
                playerTransform.position;

            toTarget.y = 0f;

            if (toTarget.sqrMagnitude <= 0.0001f)
            {
                continue;
            }

            float angle =
                Vector3.Angle(
                    playerForward,
                    toTarget.normalized
                );

            if (angle > halfAngle)
            {
                continue;
            }

            float sqrDistance = toTarget.sqrMagnitude;

            if (sqrDistance < bestDistance)
            {
                bestDistance = sqrDistance;
                bestTarget = hit.transform;
            }
        }

        return bestTarget;
    }

    private void ApplyMissileColor(
        Color baseColor,
        Color lightningColor
    )
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<Renderer>();
        }

        if (targetRenderer == null) return;

        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        targetRenderer.GetPropertyBlock(propertyBlock);

        propertyBlock.SetColor("_BaseColor", baseColor);
        propertyBlock.SetColor("_LightningColor", lightningColor);

        targetRenderer.SetPropertyBlock(propertyBlock);
    }

    public void ReflectByPlayer(Transform playerTransform)
    {
        if (reflected) return;

        Vector3 reflectDirection = Vector3.zero;

        if (playerTransform != null)
        {
            Transform targetEnemy = FindEnemyInPlayerFront(playerTransform);

            if (targetEnemy != null)
            {
                Vector3 targetPos =
                    targetEnemy.position +
                    Vector3.up * targetAimHeight;

                reflectDirection =
                    targetPos -
                    transform.position;
            }
            else
            {
                reflectDirection = playerTransform.forward;
            }
        }
        else
        {
            reflectDirection = -moveDirection;
        }

        reflectDirection.y = 0f;

        if (reflectDirection.sqrMagnitude <= 0.0001f)
        {
            reflectDirection = -moveDirection;
        }

        reflected = true;
        moveDirection = reflectDirection.normalized;
        currentSpeed = reflectedSpeed;

        transform.rotation = Quaternion.LookRotation(moveDirection);

        ApplyMissileColor(
            reflectedBaseColor,
            reflectedLightningColor
        );

        if (ignoredOwnerCollider != null && myCollider != null)
        {
            Physics.IgnoreCollision(ignoredOwnerCollider, myCollider, false);
            ignoredOwnerCollider = null;
        }
    }

    public void SetIgnoredOwner(Collider ownerCol)
    {
        myCollider = GetComponent<Collider>();
        ignoredOwnerCollider = ownerCol;

        if (ignoredOwnerCollider != null && myCollider != null)
        {
            Physics.IgnoreCollision(ignoredOwnerCollider, myCollider, true);
        }
    }

    public void ReturnToPool()
    {
        SC_EffectManager.Instance.PlayEffect("Explosion", this.transform.position);

        initialized = false;
        reflected = false;

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
