using UnityEngine;

public class SC_TutorialEnemy : MonoBehaviour
{
    [SerializeField] private SC_MoveTutorial tutorialManager;

    private bool completed;

    public void WeakAttackHit()
    {
        if (completed)
            return;

        completed = true;

        tutorialManager.WeakAttackTutorialComplete();
    }
}