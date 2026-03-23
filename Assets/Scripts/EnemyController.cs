using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    [Header("ターゲット")]
    private Transform player;

    [Header("弾")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public float shotInterval = 1.0f;
    public int maxShotCount = 5;

    [Header("移動")]
    public float moveSpeed = 2f;
    public float moveDuration = 1.5f;
    public float waitTime = 1.0f;

    [Header("HP")]
    public float maxHP = 100f;
    private float currentHP;
    public Slider hpSlider;

    private bool isDead;

    public float CurrentHP => currentHP;
    public bool IsAlive => !isDead && currentHP > 0f;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("Playerタグが見つかりません！");
        }

        currentHP = maxHP;
        UpdateHPBar();
        StartCoroutine(EnemyRoutine());
    }

    IEnumerator EnemyRoutine()
    {
        while (IsAlive)
        {
            if (player != null)
            {
                Vector3 lookPos = player.position;
                lookPos.y = transform.position.y;
                transform.LookAt(lookPos);
            }

            int shotCount = Random.Range(1, maxShotCount + 1);

            for (int i = 0; i < shotCount; i++)
            {
                if (!IsAlive)
                    yield break;

                Shoot();
                yield return new WaitForSeconds(shotInterval);
            }

            Vector3 randomDir = new Vector3(
                Random.Range(-1f, 1f),
                0f,
                Random.Range(-1f, 1f)
            ).normalized;

            float timer = 0f;

            while (timer < moveDuration)
            {
                if (!IsAlive)
                    yield break;

                transform.Translate(randomDir * moveSpeed * Time.deltaTime, Space.World);
                timer += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(waitTime);
        }
    }

    void Shoot()
    {
        if (!IsAlive)
            return;

        if (bulletPrefab == null)
            return;

        GameObject bullet = Instantiate(
            bulletPrefab,
            transform.position + transform.forward,
            Quaternion.identity
        );

        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = transform.forward * bulletSpeed;
        }
    }

    public bool TakeDamage(float damage)
    {
        if (!IsAlive)
            return true;

        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0f, maxHP);

        UpdateHPBar();

        if (currentHP <= 0f)
        {
            Die();
            return true;
        }

        return false;
    }

    void UpdateHPBar()
    {
        if (hpSlider != null)
        {
            hpSlider.value = currentHP / maxHP;
        }
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;
        StopAllCoroutines();
        Destroy(gameObject);
    }
}