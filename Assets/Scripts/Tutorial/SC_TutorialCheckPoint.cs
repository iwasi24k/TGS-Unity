using UnityEngine;

public class SC_TutorialCheckPoint : MonoBehaviour
{
    private bool reached;

    private void OnTriggerEnter(Collider other)
    {
        if (reached)
            return;

        if (other.CompareTag("Player"))
        {
            reached = true;

            gameObject.SetActive(false);
        }
    }
}