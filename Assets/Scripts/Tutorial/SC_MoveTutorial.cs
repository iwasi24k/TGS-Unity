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

    [Header("Messages")]

    [TextArea]
    [SerializeField]
    private string moveTutorial =
        "WASDで移動しよう！";

    [TextArea]
    [SerializeField]
    private string dashTutorial =
        "Shiftでダッシュしよう！\n前方のラインまで到達しよう！";

    [TextArea]
    [SerializeField]
    private string weakAttackTutorial =
        "左クリックで敵を攻撃しよう！";

    private int currentStep;

    private void Start()
    {
        currentStep = 0;

        tutorialText.text = moveTutorial;

        dashPoint.SetActive(false);
        tutorialEnemy.SetActive(false);
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
        Debug.Log("DashTutorialComplete呼び出し");

        if (currentStep != 1)
        {
            Debug.Log("currentStepが1ではない");
            return;
        }

        currentStep = 2;

        dashPoint.SetActive(false);

        tutorialEnemy.SetActive(true);

        tutorialText.text = weakAttackTutorial;

        Debug.Log("弱攻撃チュートリアルへ移行");
    }

    public void WeakAttackTutorialComplete()
    {
        if (currentStep != 2)
            return;

        currentStep = 3;

        tutorialText.text = "攻撃成功！";

        Debug.Log("弱攻撃チュートリアル完了");
    }
}