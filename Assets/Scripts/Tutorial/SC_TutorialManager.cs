using TMPro;
using UnityEngine;

public class SC_TutorialManager : MonoBehaviour
{
    private SC_Field field;
    private TMP_Text tutorialText;
    private SC_PlayerAttackManager playerAttack;

    [Header("Stage Messages")]
    [TextArea(3, 10)]
    [SerializeField]
    private string[] stageMessages;

    private int currentDisplayStage = -1;

    private bool stageCleared = false;
    private bool stageLock = false;

    private int lastCombo = 0;
    private bool lastStrong = false;


    private int lastRotateDamage = 0;
    private int lastUpperDamage = 0;
    private void Start()
    {
        field = FindFirstObjectByType<SC_Field>();
        playerAttack = FindFirstObjectByType<SC_PlayerAttackManager>();

        GameObject textObj = GameObject.Find("TutorialText");
        if (textObj != null)
            tutorialText = textObj.GetComponent<TMP_Text>();

        UpdateTutorialText();
    }

    private void Update()
    {
        if (field == null || playerAttack == null) return;

        int stage = field.GetCurrentStage();

        if (currentDisplayStage != stage)
        {
            currentDisplayStage = stage;
            stageCleared = false;
            stageLock = false;
            lastCombo = 0;
            lastStrong = false;

            UpdateTutorialText();
        }

        DetectAttack(stage);
        HandleFieldClear(stage);
    }

    // =================================================
    // ■攻撃検出（成功＋失敗）
    // =================================================
    private void DetectAttack(int stage)
    {
        int combo = playerAttack.GetCurrentComboCount();
        bool strong = playerAttack.IsNextAttackStrong();

        int rotateDamage = playerAttack.GetRotateDamage();
        int upperDamage = playerAttack.GetUppercutDamage();

        bool weakAttack = combo == 1 && lastCombo == 0;

        // ★重要：変化検出に戻す（1回だけ発火）
        bool spinAttack = rotateDamage != lastRotateDamage && rotateDamage > 0;
        bool upperAttack = upperDamage != lastUpperDamage && upperDamage > 0;

        bool strongAttack = strong;

        // 成功
        if (IsCorrectAttack(stage, weakAttack, strongAttack, spinAttack, upperAttack))
        {
            stageCleared = true;
            TryNextStage();
        }

        // 失敗
        if (IsWrongAttack(stage, weakAttack, strongAttack, spinAttack, upperAttack))
        {
            ResetStage();
        }

        lastCombo = combo;
        lastRotateDamage = rotateDamage;
        lastUpperDamage = upperDamage;
    }

    // =================================================
    // ■正解判定
    // =================================================
    private bool IsCorrectAttack(int stage, bool weak, bool strong, bool spin, bool upper)
    {
        return stage switch
        {
            2 => weak,
            3 => strong,
            4 => spin,
            5 => upper,
            _ => false
        };
    }

    // =================================================
    // ■失敗判定
    // =================================================
    private bool IsWrongAttack(int stage, bool weak, bool strong, bool spin, bool upper)
    {
        return stage switch
        {
            2 => strong || spin || upper,
            3 => weak || spin || upper,
            4 => weak || strong || upper,
            5 => weak || strong || spin,
            _ => false
        };
    }

    // =================================================
    // ■ステージリセット
    // =================================================
    private void ResetStage()
    {
        Debug.Log("Wrong attack → Stage Reset");

        stageCleared = false;
        lastCombo = 0;
        lastStrong = false;

        // 必要ならここで演出やUI戻しも可能
    }

    // =================================================
    // ■ステージ進行
    // =================================================
    private void HandleFieldClear(int stage)
    {
        if (stage == 0 || stage == 1)
        {
            if (!stageCleared && field.GetObjectCount() <= 0)
            {
                stageCleared = true;
                TryNextStage();
            }
            return;
        }

        if (!stageCleared && !stageLock)
        {
            if (field.GetEnemyCount() <= 0)
            {
                stageCleared = true;
                TryNextStage();
            }
        }
    }

    private void TryNextStage()
    {
        if (stageLock) return;

        stageLock = true;
        field.NextStage();
        Invoke(nameof(Unlock), 1.5f);
    }

    private void Unlock()
    {
        stageLock = false;
    }

    private void UpdateTutorialText()
    {
        if (tutorialText == null) return;

        if (currentDisplayStage >= 0 && currentDisplayStage < stageMessages.Length)
            tutorialText.text = stageMessages[currentDisplayStage];
        else
            tutorialText.text = "";
    }
}