using UnityEngine;

public class SC_TutorialPanel : MonoBehaviour
{
    private SC_Field field;

    private void Start()
    {
        field =
            FindFirstObjectByType<SC_Field>();

        if (field == null)
        {
            Debug.LogError(
                "SC_Field ‚ªŒ©‚Â‚©‚è‚Ü‚¹‚ñ");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        Destroy(gameObject);
    }
}