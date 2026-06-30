using UnityEngine;

public class SC_TutorialPanel : MonoBehaviour
{
    private SC_TutorialField field;

    private void Start()
    {
        field =
            FindFirstObjectByType<SC_TutorialField>();

        if (field == null)
        {
            Debug.LogError(
                "SC_TutorialField ‚ªŒ©‚Â‚©‚è‚Ü‚¹‚ñ");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        Destroy(gameObject);
    }
}