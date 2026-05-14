using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SC_ResultManager : MonoBehaviour
{
    [Header("Input Action References")]
    [SerializeField] private InputActionReference navigateAction; // 移動用
    [SerializeField] private InputActionReference submitAction;   // 決定用

    [Header("UI設定")]
    public Button[] menuButtons;       // 0:タイトルへ, 1:終了
    public string titleSceneName = "Scene_Title";

    private int currentIndex = 0;

    private void OnEnable()
    {
        // 入力アクションの有効化と登録
        if (navigateAction != null)
        {
            navigateAction.action.Enable();
            navigateAction.action.performed += OnNavigate;
        }
        if (submitAction != null)
        {
            submitAction.action.Enable();
            submitAction.action.performed += OnSubmit;
        }
    }

    private void OnDisable()
    {
        // イベントの解除
        if (navigateAction != null) navigateAction.action.performed -= OnNavigate;
        if (submitAction != null) submitAction.action.performed -= OnSubmit;
    }

    void Start()
    {
        // 起動時に最初のボタン（タイトルへ）を選択状態にする
        ApplySelection();
    }

    // --- 入力イベント ---

    private void OnNavigate(InputAction.CallbackContext context)
    {
        Vector2 direction = context.ReadValue<Vector2>();
        int move = 0;

        // 左右（または上下）の入力でインデックスを切り替え
        if (direction.x < -0.5f || direction.y < -0.5f) move = 1;  // 右・下
        else if (direction.x > 0.5f || direction.y > 0.5f) move = -1; // 左・上

        if (move != 0)
        {
            int max = menuButtons.Length;
            currentIndex = (currentIndex + move + max) % max;
            ApplySelection();
        }
    }

    public void OnSubmit(InputAction.CallbackContext context)
    {
        // 突き抜け防止：押し込んだ瞬間のみ反応
        if (!context.started) return;

        if (menuButtons.Length > 0 && currentIndex < menuButtons.Length)
        {
            menuButtons[currentIndex].onClick.Invoke();
        }
    }

    // --- ボタンのOnClickイベントに登録する関数 ---

    public void OnClickBackToTitle()
    {
        SceneManager.LoadScene(titleSceneName);
    }

    public void OnClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // --- 内部ロジック ---

    void ApplySelection()
    {
        if (menuButtons.Length > 0 && currentIndex < menuButtons.Length)
        {
            // ボタンを選択状態にする（Selected Colorが適用される）
            menuButtons[currentIndex].Select();
        }
    }
}