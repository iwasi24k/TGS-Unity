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

    // 後で作る攻撃チュートリアル管理オブジェクト
    [SerializeField] private GameObject attackTutorialManager;

    [Header("Messages")]

    [TextArea]
    [SerializeField]
    private string moveTutorial =
        "WASDで移動しよう！";

    [TextArea]
    [SerializeField]
    private string dashTutorial =
        "Shiftでダッシュしよう！\n前方のラインまで到達しよう！";


    private int currentStep;

    private void Start()
    {
        currentStep = 0;

        tutorialText.text = moveTutorial;

        dashPoint.SetActive(false);
        tutorialEnemy.SetActive(false);

        if (attackTutorialManager != null)
        {
            attackTutorialManager.SetActive(false);
        }
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


        if (attackTutorialManager != null)
        {
            attackTutorialManager.SetActive(true);
        }

        Debug.Log("ダッシュチュートリアル完了");
    }

    public void TutorialComplete()
    {
        currentStep = 999;

        tutorialText.text = "チュートリアル完了！";

        Debug.Log("全チュートリアル完了");
    }
}