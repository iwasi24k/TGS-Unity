using UnityEngine;

public class SC_TutorialEnemy : MonoBehaviour
{
    private Vector3 startPosition;
    private Quaternion startRotation;

    private Rigidbody rb;

    private SC_EnemyStatusManager status;

    private void Start()
    {
        startPosition =
            transform.position;

        startRotation =
            transform.rotation;

        rb =
            GetComponent<Rigidbody>();

        status =
            GetComponent<SC_EnemyStatusManager>();
    }

    public void ResetEnemy()
    {
        transform.position =
            startPosition;

        transform.rotation =
            startRotation;

        if (rb != null)
        {
            rb.linearVelocity =
                Vector3.zero;

            rb.angularVelocity =
                Vector3.zero;
        }

        if (status != null)
        {
            status.ResetEnemyStatus();

            status.ChangeState(0);
        }

        Debug.Log("チュートリアル敵リセット");
    }
}