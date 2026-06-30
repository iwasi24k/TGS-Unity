using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SC_TutorialManager : MonoBehaviour
{
    private SC_TutorialField field;
    private TMP_Text tutorialText;
    private SC_PlayerAttackManager playerAttack;
    private SC_PlayerTarget playerTarget;

    // チャージ攻撃取得用
    private SC_PlayerChargeAttack chargeAttack;

    // =================================================
    // ■チュートリアル種別
    // =================================================
    public enum TutorialType
    {
        Move,
        Dash,
        WeakCombo,
        ChargeAttack,
        Target,
        Combo,
        Melee,
        Multi,
        Reflect,
        Next
    }

    // =================================================
    // ■Stageデータ
    // =================================================
    [System.Serializable]
    public class StageUIData
    {
        [Header("Tutorial Type")]
        public TutorialType tutorialType;

        [Header("Success Message")]
        [TextArea(3, 10)]
        public string message;

        [Header("Fail Message")]
        [TextArea(2, 5)]
        public string failMessage;

        [Header("Wait Time")]
        public float waitTime = 5f;

        [Header("Images")]
        public StageImageData[] images;
    }

    // =================================================
    // ■画像データ
    // =================================================
    [System.Serializable]
    public class StageImageData
    {
        public Texture2D texture;
        public Vector2 position;
        public Vector2 size = new Vector2(200, 200);
        public float rotation;
    }

    // =================================================
    // ■Inspector
    // =================================================
    [Header("Stage Data")]
    [SerializeField]
    private StageUIData[] stages;

    // =================================================
    // ■Stage状態
    // =================================================
    private int currentDisplayStage = -1;
    private float stageTimer = 0f;

    private bool stageCleared = false;
    private bool stageLock = false;

    private int lastCombo = 0;

    // =================================================
    // ■UI
    // =================================================
    private Image[] tutorialImages;
    private Sprite[] spriteCache = new Sprite[5];

    // =================================================
    // ■失敗管理
    // =================================================
    private bool isFailing = false;

    [SerializeField]
    private float failDelay = 1.5f;

    private string cachedMessage = "";

    // =================================================
    private void Start()
    {
        field =
            FindFirstObjectByType<SC_TutorialField>();

        playerAttack =
            FindFirstObjectByType<SC_PlayerAttackManager>();

        playerTarget =
            FindFirstObjectByType<SC_PlayerTarget>();

        // =========================================
        // チャージ攻撃取得
        // =========================================
        SC_PlayerChargeAttack[] charges =
            Resources.FindObjectsOfTypeAll<SC_PlayerChargeAttack>();

        if (charges.Length > 0)
        {
            chargeAttack =
                charges[0];

            Debug.Log(
                "ChargeAttack取得 : " +
                chargeAttack.name);
        }
        else
        {
            Debug.LogError(
                "SC_PlayerChargeAttackが見つかりません");
        }

        tutorialText =
            GameObject.Find("TutorialText")
            ?.GetComponent<TMP_Text>();

        tutorialImages =
            new Image[5];

        tutorialImages[0] =
            GameObject.Find("TutorialImage1")
            ?.GetComponent<Image>();

        tutorialImages[1] =
            GameObject.Find("TutorialImage2")
            ?.GetComponent<Image>();

        tutorialImages[2] =
            GameObject.Find("TutorialImage3")
            ?.GetComponent<Image>();

        tutorialImages[3] =
            GameObject.Find("TutorialImage4")
            ?.GetComponent<Image>();

        tutorialImages[4] =
            GameObject.Find("TutorialImage5")
            ?.GetComponent<Image>();

        ForceInitUI();

        currentDisplayStage = -1;

        stageCleared = false;
        stageLock = false;

        lastCombo = 0;

        isFailing = false;

        cachedMessage = "";

        UpdateTutorialText();
        UpdateTutorialUI(currentDisplayStage);

        Debug.Log("TutorialManager Start");
    }

    // =================================================
    // ■UI初期化
    // =================================================
    private void ForceInitUI()
    {
        for (int i = 0; i < tutorialImages.Length; i++)
        {
            if (tutorialImages[i] == null)
            {
                continue;
            }

            RectTransform rt =
                tutorialImages[i]
                .GetComponent<RectTransform>();

            rt.anchorMin =
                new Vector2(0.5f, 0.5f);

            rt.anchorMax =
                new Vector2(0.5f, 0.5f);

            rt.pivot =
                new Vector2(0.5f, 0.5f);

            tutorialImages[i]
                .gameObject
                .SetActive(false);
        }
    }

    // =================================================
    private void Update()
    {
        if (field == null ||
            playerAttack == null)
        {
            return;
        }

        int stage =
            field.GetCurrentStage();

        if (currentDisplayStage != stage)
        {
            currentDisplayStage = stage;

            stageCleared = false;
            stageLock = false;

            if (playerAttack != null)
            {
                playerAttack.ResetCombo();
            }

            lastCombo = 0;

            stageTimer = 0f;

            isFailing = false;

            UpdateTutorialText();
            UpdateTutorialUI(stage);

            Debug.Log(
                "Stage Change : " +
                currentDisplayStage);
        }

        DetectTutorialProgress(stage);
    }

    // =================================================
    // ■チュートリアル進行判定
    // =================================================
    private void DetectTutorialProgress(int stage)
    {
        if (stages == null)
        {
            return;
        }

        if (stage < 0 ||
            stage >= stages.Length)
        {
            return;
        }

        int combo =
            playerAttack.GetCurrentComboCount();

        TutorialType type =
            stages[stage].tutorialType;

        switch (type)
        {
            // =========================================
            // Move
            // =========================================
            case TutorialType.Move:

                if (!stageCleared &&
                    field.GetObjectCount() <= 0)
                {
                    stageCleared = true;
                    TryNextStage();
                }

                break;

            // =========================================
            // Dash
            // =========================================
            case TutorialType.Dash:

                if (!stageCleared &&
                    field.GetObjectCount() <= 0)
                {
                    stageCleared = true;
                    TryNextStage();
                }

                break;

            // =========================================
            // WeakCombo
            // =========================================
            case TutorialType.WeakCombo:

                if (!stageCleared &&combo >= 3)
                {
                    Debug.Log("WeakCombo Success");

                    stageCleared = true;
                    TryNextStage();

                    return;
                }

                break;

            // =========================================
            // ChargeAttack
            // =========================================
            case TutorialType.ChargeAttack:

                if (!stageCleared &&
                    combo > lastCombo)
                {
                    Debug.Log("Charge Tutorial : Weak Attack");

                    FailStage(
                        stages[currentDisplayStage]
                        .failMessage);

                    return;
                }

                if (!stageCleared &&
                    chargeAttack != null &&
                    chargeAttack.GetWasHit())
                {
                    Debug.Log("Charge Attack Success");

                    stageCleared = true;
                    TryNextStage();

                    return;
                }


                if (!stageCleared &&
                    field.GetEnemyCount() <= 0)
                {
                    Debug.Log("Charge Attack Failed");

                    FailStage(
                        stages[currentDisplayStage]
                        .failMessage);

                    return;
                }

                break;

            // =========================================
            // Target
            // =========================================
            case TutorialType.Target:

                if (!stageCleared &&
                    playerTarget != null &&
                    playerTarget.GetCurrentTarget() != null)
                {
                    stageCleared = true;
                    TryNextStage();
                }

                break;

            // =========================================
            // Combo
            // =========================================
            case TutorialType.Combo:

                if (!stageCleared &&
                    ComboManager.Instance != null &&
                    ComboManager.Instance.GetComboCount() >= 2)
                {
                    stageCleared = true;
                    TryNextStage();
                }

                break;

            // =========================================
            // Melee
            // =========================================
            case TutorialType.Melee:

                if (!stageCleared &&
                    field.GetEnemyCount() <= 0)
                {
                    Debug.Log("Melee Complete");

                    stageCleared = true;
                    TryNextStage();

                    return;
                }

                break;
            // =========================================
            // Multi
            // =========================================
            case TutorialType.Multi:

                if (!stageCleared &&
                    field.GetEnemyCount() <= 0)
                {
                    Debug.Log("Melee Complete");

                    stageCleared = true;
                    TryNextStage();

                    return;
                }

                break;
            // =========================================
            // Reflect
            // =========================================
            case TutorialType.Reflect:

                if (!stageCleared &&
                    field.GetEnemyCount() <= 0)
                {
                    Debug.Log("Melee Complete");

                    stageCleared = true;
                    TryNextStage();

                    return;
                }

                break;

            // =========================================
            // Next
            // =========================================
            case TutorialType.Next:


                break;

        }

        lastCombo = combo;
    }
    // =================================================
    // ■ステージ進行
    // =================================================
    private void TryNextStage()
    {
        if (stageLock)
        {
            return;
        }

        stageLock = true;

        field.NextStage();

        Invoke(nameof(Unlock), 1.5f);
    }

    private void Unlock()
    {
        stageLock = false;
    }

    // =================================================
    // ■テキスト更新
    // =================================================
    private void UpdateTutorialText()
    {
        if (tutorialText == null)
        {
            return;
        }

        if (stages != null &&
            currentDisplayStage >= 0 &&
            currentDisplayStage < stages.Length)
        {
            tutorialText.text =
                stages[currentDisplayStage].message;
        }
        else
        {
            tutorialText.text = "";
        }
    }

    // =================================================
    // ■UI更新
    // =================================================
    private void UpdateTutorialUI(int stage)
    {
        if (stages == null)
        {
            return;
        }

        if (stage < 0 ||
            stage >= stages.Length)
        {
            return;
        }

        for (int i = 0; i < tutorialImages.Length; i++)
        {
            if (tutorialImages[i] == null)
            {
                continue;
            }

            tutorialImages[i]
                .gameObject
                .SetActive(false);
        }

        var images =
            stages[stage].images;

        if (images == null)
        {
            return;
        }

        int count =
            Mathf.Min(
                images.Length,
                tutorialImages.Length);

        for (int i = 0; i < count; i++)
        {
            var data = images[i];
            var ui = tutorialImages[i];

            if (ui == null ||
                data.texture == null)
            {
                continue;
            }

            RectTransform rt =
                ui.GetComponent<RectTransform>();

            if (spriteCache[i] == null ||
                spriteCache[i].texture != data.texture)
            {
                spriteCache[i] =
                    Sprite.Create(
                        data.texture,
                        new Rect(
                            0,
                            0,
                            data.texture.width,
                            data.texture.height),
                        new Vector2(0.5f, 0.5f));
            }

            ui.sprite =
                spriteCache[i];

            ui.gameObject.SetActive(true);

            rt.anchoredPosition =
                data.position;

            rt.sizeDelta =
                data.size;

            rt.localRotation =
                Quaternion.Euler(
                    0,
                    0,
                    data.rotation);
        }
    }

    // =================================================
    // ■失敗処理
    // =================================================
    private void FailStage(string warningMessage)
    {
        Debug.Log(
            "FailStage呼び出し : " +
            warningMessage);

        if (isFailing)
        {
            return;
        }

        isFailing = true;

        if (tutorialText != null)
        {
            cachedMessage =
                tutorialText.text;

            tutorialText.text =
                "<color=red>" +
                warningMessage +
                "</color>";
        }

        Invoke(
            nameof(ReloadStage),
            failDelay);
    }

    // =================================================
    // ■ステージリセット
    // =================================================
    private void ReloadStage()
    {
        field.ResetStage();

        if (tutorialText != null)
        {
            if (stages != null &&
                currentDisplayStage >= 0 &&
                currentDisplayStage < stages.Length)
            {
                tutorialText.text =
                    stages[currentDisplayStage]
                    .message;
            }
        }

        isFailing = false;
    }

}
