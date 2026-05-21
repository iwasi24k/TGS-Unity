using UnityEngine;

public class SC_FallingMissile : MonoBehaviour
{
    private Vector3 targetPosition;
    private float fallTime;
    private float timer;
    private Vector3 startPosition;
    private GameObject warningMark;

    public void Init(Vector3 targetPosition, float height, float fallTime, GameObject warningMarkPrefab)
    {
        this.targetPosition = targetPosition;
        this.fallTime = fallTime;
        timer = 0f;

        startPosition = targetPosition + Vector3.up * height;
        transform.position = startPosition;

        if (warningMarkPrefab != null)
        {
            warningMark = Instantiate(
                warningMarkPrefab,
                targetPosition,
                Quaternion.identity
            );
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        float t = timer / fallTime;
        t = Mathf.Clamp01(t);

        transform.position = Vector3.Lerp(startPosition, targetPosition, t);

        if (t >= 1.0f)
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (warningMark != null)
        {
            Destroy(warningMark);
        }

        Collider[] hits = Physics.OverlapSphere(targetPosition, 1.5f);

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 dir = hit.transform.position - targetPosition;
                dir.y = 0f;
                dir.Normalize();

                rb.AddForce(dir * 10.0f, ForceMode.Impulse);
            }
        }

        Destroy(gameObject);
    }
}
