using TMPro;
using UnityEngine;

public class SC_AttackTutorial : MonoBehaviour
{
    public enum AttackTutorialStep
    {
        WeakAttack,
        StrongAttack,
        RotateCombo,
        UppercutCombo,
        ChainTutorial,
        Complete
    }

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI TutorialText;

    [Header("Enemy Reset")]
    [SerializeField] private SC_TutorialEnemyReset enemyReset;

    [Header("Tutorial Manager")]
    private SC_MoveTutorial MoveTutorial;

    [Header("Timing Settings")]
    [SerializeField] private float successNextStepDelay = 0.6f;
    [SerializeField] private float successResetDelay = 0.3f;
    [SerializeField] private float failResetDelay = 0.5f;

    [Header("Chain Tutorial")]
    [SerializeField] private GameObject chainTutorialRoot;

    [Header("Messages")]
    [TextArea][SerializeField] private string weakAttackMessage = "ç∂ÉNÉäÉbÉNÇ≈ìGÇçUåÇÇµÇÊÇ§ÅI";
    [TextArea][SerializeField] private string strongAttackMessage = "âEÉNÉäÉbÉNÇ≈ã≠çUåÇÇµÇÊÇ§ÅI";
    [TextArea][SerializeField] private string rotateComboMessage = "é„çUåÇ Å® ã≠çUåÇÇê¨å˜Ç≥ÇπÇÊÇ§ÅI";
    [TextArea][SerializeField] private string uppercutComboMessage = "é„çUåÇ Å® é„çUåÇ Å® ã≠çUåÇÇê¨å˜Ç≥ÇπÇÊÇ§ÅI";
    [TextArea][SerializeField] private string chainTutorialMessage = "ìGÇêÅÇ´îÚÇŒÇµÇƒëºÇÃìGÇ…Ç‘Ç¬ÇØÇÊÇ§ÅI";

    private AttackTutorialStep currentStep;
    private bool destroyEnemyOnReset = false;

    private void Start()
    {
        // =========================
        // MoveTutorial ÇñºëOÇ≈é©ìÆéÊìæ
        // =========================
        if (TutorialText == null)
        {
            var go = GameObject.Find("TutorialText");
            if (go != null)
                TutorialText = go.GetComponent<TextMeshProUGUI>();
        }
    }

    private void OnEnable()
    {
        currentStep = AttackTutorialStep.WeakAttack;

        if (chainTutorialRoot != null)
            chainTutorialRoot.SetActive(false);

        ShowCurrentTutorial();
    }

    private void ShowCurrentTutorial()
    {
        switch (currentStep)
        {
            case AttackTutorialStep.WeakAttack:
                TutorialText.text = weakAttackMessage;
                break;

            case AttackTutorialStep.StrongAttack:
                TutorialText.text = strongAttackMessage;
                break;

            case AttackTutorialStep.RotateCombo:
                TutorialText.text = rotateComboMessage;
                break;

            case AttackTutorialStep.UppercutCombo:
                TutorialText.text = uppercutComboMessage;
                break;

            case AttackTutorialStep.ChainTutorial:
                TutorialText.text = chainTutorialMessage;
                break;

            case AttackTutorialStep.Complete:
                TutorialText.text = "çUåÇÉ`ÉÖÅ[ÉgÉäÉAÉãäÆóπÅI";
                break;
        }
    }

    public AttackTutorialStep GetCurrentStep() => currentStep;

    public void OnAttackHit(AttackType attackType)
    {
        Debug.Log($"OnAttackHit : {attackType}");

        switch (currentStep)
        {
            case AttackTutorialStep.WeakAttack:
                if (attackType == AttackType.Weak1)
                {
                    currentStep = AttackTutorialStep.StrongAttack;
                    ShowCurrentTutorial();
                    Invoke(nameof(SuccessResetEnemy), successResetDelay);
                }
                else Invoke(nameof(FailResetEnemy), failResetDelay);
                break;

            case AttackTutorialStep.StrongAttack:
                if (attackType == AttackType.Strong)
                {
                    currentStep = AttackTutorialStep.RotateCombo;
                    ShowCurrentTutorial();
                    Invoke(nameof(SuccessResetEnemy), successResetDelay);
                }
                else Invoke(nameof(FailResetEnemy), failResetDelay);
                break;

            case AttackTutorialStep.RotateCombo:
                if (attackType == AttackType.Rotate)
                {
                    currentStep = AttackTutorialStep.UppercutCombo;
                    ShowCurrentTutorial();
                    Invoke(nameof(SuccessResetEnemy), successResetDelay);
                }
                else Invoke(nameof(FailResetEnemy), failResetDelay);
                break;

            case AttackTutorialStep.UppercutCombo:
                if (attackType == AttackType.Uppercut)
                {
                    Invoke(nameof(StartChainTutorial), successNextStepDelay);
                    Invoke(nameof(SuccessResetEnemy), successResetDelay);
                    destroyEnemyOnReset = true;
                }
                else Invoke(nameof(FailResetEnemy), failResetDelay);
                break;
        }
    }

    private void SuccessResetEnemy()
    {
        if (enemyReset == null) return;

        enemyReset.ForceResetAllMotion();

        if (destroyEnemyOnReset)
        {
            Destroy(enemyReset.gameObject);
            destroyEnemyOnReset = false;
            return;
        }

        enemyReset.ResetEnemy();
    }

    private void FailResetEnemy()
    {
        if (enemyReset == null) return;

        enemyReset.ForceResetAllMotion();
        enemyReset.ResetEnemy();
    }

    private void StartChainTutorial()
    {
        currentStep = AttackTutorialStep.ChainTutorial;

        if (chainTutorialRoot != null)
            chainTutorialRoot.SetActive(true);

        ShowCurrentTutorial();
    }

    public void OnChainSuccess()
    {
        if (currentStep != AttackTutorialStep.ChainTutorial)
            return;

        currentStep = AttackTutorialStep.Complete;
        ShowCurrentTutorial();

        MoveTutorial?.TutorialComplete();
    }
}