using UnityEngine;

public class SC_TutorialEnemyReset : MonoBehaviour
{
    private Vector3 startPosition;
    private Quaternion startRotation;

    private SC_EnemyStatusManager enemyStatus;
    private Rigidbody rb;

    private void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;

        enemyStatus = GetComponent<SC_EnemyStatusManager>();
        rb = GetComponent<Rigidbody>();
    }

    // ★追加：物理状態完全停止
    public void ForceResetAllMotion()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.Sleep();
        }

        // 状態系があれば追加で止める余地
    }

    public void ResetEnemy()
    {
        if (enemyStatus != null)
        {
            enemyStatus.ResetEnemyStatus();
        }

        transform.position = startPosition;
        transform.rotation = startRotation;

        gameObject.SetActive(false);
        gameObject.SetActive(true);

        Debug.Log("チュートリアル敵リセット");
    }
}