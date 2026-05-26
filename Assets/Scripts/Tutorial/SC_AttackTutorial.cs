using TMPro;
using UnityEngine;

public class SC_AttackTutorial : MonoBehaviour
{
    public enum AttackTutorialStep
    {
        Targeting,      // ★追加
        WeakAttack,
        StrongAttack,
        RotateCombo,
        UppercutCombo,
        ChainTutorial,
        Complete
    }

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI tutorialText;

    [Header("Enemy Reset")]
    [SerializeField] private SC_TutorialEnemyReset enemyReset;

    [Header("Tutorial Manager")]
    [SerializeField] private SC_MoveTutorial moveTutorial;

    [Header("Timing Settings")]
    [SerializeField] private float successNextStepDelay = 0.6f;
    [SerializeField] private float successResetDelay = 0.3f;
    [SerializeField] private float failResetDelay = 0.5f;

    [Header("Chain Tutorial")]
    [SerializeField] private GameObject chainTutorialRoot;

    [Header("Messages")]
    [TextArea][SerializeField] private string targetingMessage = "ターゲットをロックオンしよう！";
    [TextArea][SerializeField] private string weakAttackMessage = "左クリックで敵を攻撃しよう！";
    [TextArea][SerializeField] private string strongAttackMessage = "右クリックで強攻撃しよう！";
    [TextArea][SerializeField] private string rotateComboMessage = "弱攻撃 → 強攻撃を成功させよう！";
    [TextArea][SerializeField] private string uppercutComboMessage = "弱攻撃 → 弱攻撃 → 強攻撃を成功させよう！";
    [TextArea][SerializeField] private string chainTutorialMessage = "敵を吹き飛ばして他の敵にぶつけよう！";

    private AttackTutorialStep currentStep;
    private bool destroyEnemyOnReset = false;

    private void OnEnable()
    {
        currentStep = AttackTutorialStep.Targeting;

        if (chainTutorialRoot != null)
            chainTutorialRoot.SetActive(false);

        ShowCurrentTutorial();
    }

    private void ShowCurrentTutorial()
    {
        switch (currentStep)
        {
            case AttackTutorialStep.Targeting:
                tutorialText.text = targetingMessage;
                break;

            case AttackTutorialStep.WeakAttack:
                tutorialText.text = weakAttackMessage;
                break;

            case AttackTutorialStep.StrongAttack:
                tutorialText.text = strongAttackMessage;
                break;

            case AttackTutorialStep.RotateCombo:
                tutorialText.text = rotateComboMessage;
                break;

            case AttackTutorialStep.UppercutCombo:
                tutorialText.text = uppercutComboMessage;
                break;

            case AttackTutorialStep.ChainTutorial:
                tutorialText.text = chainTutorialMessage;
                break;

            case AttackTutorialStep.Complete:
                tutorialText.text = "攻撃チュートリアル完了！";
                break;
        }
    }

    public AttackTutorialStep GetCurrentStep() => currentStep;

    // =========================
    // ターゲット成功通知（外部から呼ぶ）
    // =========================
    public void OnTargetingSuccess()
    {
        if (currentStep != AttackTutorialStep.Targeting)
            return;

        currentStep = AttackTutorialStep.WeakAttack;
        ShowCurrentTutorial();
    }

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

        moveTutorial?.TutorialComplete();
    }
}