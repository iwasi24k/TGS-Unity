using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SC_TutorialManager : MonoBehaviour
{
    private SC_Field field;
    private TMP_Text tutorialText;
    private SC_PlayerAttackManager playerAttack;

    // =================================================
    // ■Stageデータ
    // =================================================
    [System.Serializable]
    public class StageUIData
    {
        [TextArea(3, 10)]
        public string message;

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

    [Header("Stage Data")]
    [SerializeField]
    private StageUIData[] stages;

    private int currentDisplayStage = -1;

    private bool stageCleared = false;
    private bool stageLock = false;

    // -------------------------
    // 攻撃検出用
    // -------------------------
    private int lastCombo = 0;
    private bool lastStrongState = false;

    // UI
    private Image[] tutorialImages;
    private Sprite[] spriteCache = new Sprite[5];

    // =================================================
    private void Start()
    {
        field = FindFirstObjectByType<SC_Field>();
        playerAttack = FindFirstObjectByType<SC_PlayerAttackManager>();

        tutorialText = GameObject.Find("TutorialText")?.GetComponent<TMP_Text>();

        tutorialImages = new Image[5];

        tutorialImages[0] = GameObject.Find("TutorialImage1")?.GetComponent<Image>();
        tutorialImages[1] = GameObject.Find("TutorialImage2")?.GetComponent<Image>();
        tutorialImages[2] = GameObject.Find("TutorialImage3")?.GetComponent<Image>();
        tutorialImages[3] = GameObject.Find("TutorialImage4")?.GetComponent<Image>();
        tutorialImages[4] = GameObject.Find("TutorialImage5")?.GetComponent<Image>();

        ForceInitUI();

        UpdateTutorialText();
        UpdateTutorialUI(currentDisplayStage);
    }

    // =================================================
    private void ForceInitUI()
    {
        for (int i = 0; i < tutorialImages.Length; i++)
        {
            if (tutorialImages[i] == null) continue;

            RectTransform rt = tutorialImages[i].GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            tutorialImages[i].gameObject.SetActive(false);
        }
    }

    // =================================================
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
            lastStrongState = false;

            UpdateTutorialText();
            UpdateTutorialUI(stage);
        }

        DetectAttack(stage);
        HandleFieldClear(stage);
    }

    // =================================================
    // ■攻撃検出（安定版）
    // =================================================
    private void DetectAttack(int stage)
    {
        int combo = playerAttack.GetCurrentComboCount();
        bool strong = playerAttack.IsNextAttackStrong();

        // -------------------------
        // 弱攻撃（combo増加）
        // -------------------------
        bool weakAttack = combo > lastCombo;

        if (weakAttack && stage == 2)
        {
            stageCleared = true;
            TryNextStage();
        }

        // -------------------------
        // 強攻撃（立ち上がり検出）
        // -------------------------
        bool strongAttack = strong && !lastStrongState;

        if (strongAttack && stage >= 3)
        {
            stageCleared = true;
            TryNextStage();
        }

        // 更新
        lastCombo = combo;
        lastStrongState = strong;
    }

    // =================================================
    // ■敵処理
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

    // =================================================
    // ■ステージ進行
    // =================================================
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

    // =================================================
    // ■テキスト更新
    // =================================================
    private void UpdateTutorialText()
    {
        if (tutorialText == null) return;

        if (currentDisplayStage >= 0 && currentDisplayStage < stages.Length)
        {
            tutorialText.text = stages[currentDisplayStage].message;
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
        if (stages == null) return;
        if (stage < 0 || stage >= stages.Length) return;

        for (int i = 0; i < tutorialImages.Length; i++)
        {
            if (tutorialImages[i] == null) continue;
            tutorialImages[i].gameObject.SetActive(false);
        }

        var images = stages[stage].images;
        if (images == null) return;

        int count = Mathf.Min(images.Length, tutorialImages.Length);

        for (int i = 0; i < count; i++)
        {
            var data = images[i];
            var ui = tutorialImages[i];

            if (ui == null || data.texture == null) continue;

            RectTransform rt = ui.GetComponent<RectTransform>();

            if (spriteCache[i] == null || spriteCache[i].texture != data.texture)
            {
                spriteCache[i] = Sprite.Create(
                    data.texture,
                    new Rect(0, 0, data.texture.width, data.texture.height),
                    new Vector2(0.5f, 0.5f)
                );
            }

            ui.sprite = spriteCache[i];
            ui.gameObject.SetActive(true);

            rt.anchoredPosition = data.position;
            rt.sizeDelta = data.size;
            rt.localRotation = Quaternion.Euler(0, 0, data.rotation);
        }
    }
}