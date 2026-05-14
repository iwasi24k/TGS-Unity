using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SC_TutorialManager : MonoBehaviour
{
    [Header("Input Action References")]
    [SerializeField] private InputActionReference navigateAction; // 移動用
    [SerializeField] private InputActionReference submitAction;   // 決定用

    [Header("遷移先シーン名")]
    public string mainGameSceneName = "Scene_Game";
    public string tutorialSceneName = "Scene_Tutorial";

    // --- 内部変数 ---
    private int currentIndex = 0;       // 現在選択されているボタンの配列番号
    private bool isPopupActive = false; //

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GoTitle()
    {
        SceneManager.LoadScene(mainGameSceneName);
    }

    public void GoMainGame()
    {
        SceneManager.LoadScene(tutorialSceneName);
    }
}
