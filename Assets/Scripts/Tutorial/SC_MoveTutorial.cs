using TMPro;
using UnityEngine;

public class SC_MoveTutorial : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI tutorialText;

    [Header("Move Tutorial Points")]
    [SerializeField] private GameObject Front;
    [SerializeField] private GameObject Back;
    [SerializeField] private GameObject Left;
    [SerializeField] private GameObject Right;

    [Header("Dash Tutorial Point")]
    [SerializeField] private GameObject dashPoint;

    [Header("Attack Tutorial")]
    [SerializeField] private GameObject tutorialEnemy;

    [SerializeField] private GameObject attackTutorialManager;

    [Header("Messages")]
    [TextArea]
    [SerializeField] private string moveTutorial = "WASDで移動しよう！";

    [TextArea]
    [SerializeField]
    private string dashTutorial =
        "Shiftでダッシュしよう！\n前方のラインまで到達しよう！";

    private int currentStep;

    private void Start()
    {
        // =========================
        // UI（名前で取得）
        // =========================
        if (tutorialText == null)
        {
            var go = GameObject.Find("TutorialText");
            if (go != null)
                tutorialText = go.GetComponent<TextMeshProUGUI>();
        }

        // =========================
        // Moveポイント（名前管理）
        // =========================
        Front = Front != null ? Front : GameObject.Find("Front");
        Back = Back != null ? Back : GameObject.Find("Back");
        Left = Left != null ? Left : GameObject.Find("Left");
        Right = Right != null ? Right : GameObject.Find("Right");

        dashPoint = dashPoint != null ? dashPoint : GameObject.Find("DashPoint");

        // =========================
        // Prefabロード（確実版）
        // =========================
        if (tutorialEnemy == null)
        {
            GameObject prefab = Resources.Load<GameObject>("PF_TutorialEnemy");

            if (prefab != null)
            {
                tutorialEnemy = Instantiate(prefab);
                tutorialEnemy.name = "PF_TutorialEnemy";
                tutorialEnemy.SetActive(false);
            }
            else
            {
                Debug.LogError("PF_TutorialEnemy がResourcesに存在しません");
            }
        }

        if (attackTutorialManager == null)
        {
            GameObject prefab = Resources.Load<GameObject>("PF_AttackTutorialManager");

            if (prefab != null)
            {
                attackTutorialManager = Instantiate(prefab);
                attackTutorialManager.name = "PF_AttackTutorialManager";
                attackTutorialManager.SetActive(false);
            }
            else
            {
                Debug.LogError("PF_AttackTutorialManager がResourcesに存在しません");
            }
        }

        // =========================
        // 初期化
        // =========================
        currentStep = 0;

        if (tutorialText != null)
            tutorialText.text = moveTutorial;

        if (dashPoint != null)
            dashPoint.SetActive(false);

        if (tutorialEnemy != null)
            tutorialEnemy.SetActive(false);

        if (attackTutorialManager != null)
            attackTutorialManager.SetActive(false);
    }

    private void Update()
    {
        if (currentStep == 0)
        {
            CheckMoveTutorialComplete();
        }
    }

    private void CheckMoveTutorialComplete()
    {
        if (!Front.activeSelf &&
            !Back.activeSelf &&
            !Left.activeSelf &&
            !Right.activeSelf)
        {
            MoveTutorialComplete();
        }
    }

    private void MoveTutorialComplete()
    {
        currentStep = 1;

        tutorialText.text = dashTutorial;

        dashPoint.SetActive(true);

        Debug.Log("移動チュートリアル完了");
    }

    public void DashTutorialComplete()
    {
        if (currentStep != 1)
            return;

        currentStep = 2;

        dashPoint.SetActive(false);

        tutorialEnemy.SetActive(true);
        attackTutorialManager.SetActive(true);

        Debug.Log("ダッシュチュートリアル完了");
    }

    public void TutorialComplete()
    {
        currentStep = 999;
        tutorialText.text = "チュートリアル完了！";
        Debug.Log("全チュートリアル完了");
    }
}