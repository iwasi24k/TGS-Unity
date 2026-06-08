using UnityEngine;

public class SC_DashTutorialPoint : MonoBehaviour
{
    [SerializeField] private SC_MoveTutorial tutorialManager;

    private bool reached;

    private void OnTriggerEnter(Collider other)
    {
        if (reached)
            return;

        if (other.CompareTag("Player"))
        {
            reached = true;

            tutorialManager.DashTutorialComplete();

            gameObject.SetActive(false);
        }
    }
}