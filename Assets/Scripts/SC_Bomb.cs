using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("爆発範囲")]
    [SerializeField] private float explosionRadius = 5f;

    [Header("爆発力")]
    [SerializeField] private float explosionPower = 15f;

    [Header("爆発エフェクト")]
    [SerializeField] private GameObject explosionEffect;

    private bool exploded = false;

    //========================
    // 衝突時
    //========================
    private void OnCollisionEnter(Collision collision)
    {
        // Enemyにぶつかった
        if (collision.gameObject.CompareTag("Enemy"))
        {
            SC_EnemyStatusManager enemy = collision.gameObject.GetComponent<SC_EnemyStatusManager>();

            // 吹っ飛び中なら爆発
            if (enemy != null && enemy.IsBlownAway())
            {
                Explode();
            }
        }
    }

    //========================
    // 爆発
    //========================
    public void Explode()
    {
        if (exploded) return;

        exploded = true;

        // エフェクト
        if (explosionEffect != null)
        {
            Instantiate(
                explosionEffect,
                transform.position,
                Quaternion.identity
            );
        }

        // 範囲取得
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            explosionRadius
        );

        foreach (Collider hit in hits)
        {
            //========================
            // 吹っ飛ばし
            //========================

            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.AddExplosionForce(
                    explosionPower,
                    transform.position,
                    explosionRadius,
                    1f,
                    ForceMode.Impulse
                );
            }

            //========================
            // 敵ダメージ
            //========================

            if (hit.CompareTag("Enemy"))
            {
                SC_EnemyStatusManager enemy =
                    hit.GetComponent<SC_EnemyStatusManager>();

                if (enemy != null)
                {
                    enemy.TakeDamage(
                        20,
                        transform.position,
                        true,
                        AttackType.Strong
                    );
                }
            }

            //========================
            // 爆弾連鎖
            //========================

            if (hit.CompareTag("Bomb"))
            {
                Bomb bomb = hit.GetComponent<Bomb>();

                if (bomb != null)
                {
                    bomb.Explode();
                }
            }
        }

        Destroy(gameObject);
    }

    //========================
    // 範囲可視化
    //========================
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            explosionRadius
        );
    }
}