using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 3f;

    // すでにヒットしたかどうか
    private bool hasHit = false;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        // すでに当たっていたら何もしない
        if (hasHit) return;

        // プレイヤーに当たった場合
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
            }
        }

        // ヒット済みにする（これが重要）
        hasHit = true;

        // 弾を削除
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        // すでに当たっていたら何もしない
        if (hasHit) return;

        if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Bullet"))
        {
            return;
        }
        // プレイヤーに当たった場合
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.gameObject.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
            }
        }

        // ヒット済みにする（これが重要）
        hasHit = true;

        // 弾を削除
        Destroy(gameObject);
    }
}