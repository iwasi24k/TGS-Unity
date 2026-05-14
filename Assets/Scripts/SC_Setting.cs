using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SC_Setting : MonoBehaviour
{
    [Header("Ref")]
    [Tooltip("ポーズ中に動きを止めたいスクリプトを登録")]
    [SerializeField] private MonoBehaviour[] pauseTarget;
    [Tooltip("設定画面のUI親オブジェクト")]
    [SerializeField] private GameObject settingUI;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference iaSettingtoggle; // 設定を開くボタン
    [SerializeField] private InputActionReference navigateAction;   // 十字キー・スティック移動
    [SerializeField] private InputActionReference submitAction;     // 決定ボタン

    [Header("UI Selectable")]
    // 0:オプション, 1:タイトルへ戻る, 2:チュートリアルへ戻る
    [Tooltip("中央に並んでいるメインボタンを順番に登録")]
    public Button[] mainSettingButtons;
    [Tooltip("左上にある閉じるボタンを登録")]
    public Button closeButton;

    // 内部状態管理用
    private bool isPaused = false;
    private CursorLockMode previousLockMode;
    private bool previousCursorVisible;

    private int currentIndex = 0;               // 中央ボタンの何番目を選択中か
    private bool isCloseButtonSelected = false; // 現在「閉じるボタン」にフォーカスがあるか

    void Start()
    {
        // 起動時は設定画面を閉じておく
        if (settingUI != null) CloseSettings();
    }

    void OnEnable()
    {
        // Input Systemのアクションを有効化し、イベントを購読
        if (navigateAction != null) navigateAction.action.Enable();
        if (submitAction != null) submitAction.action.Enable();

        navigateAction.action.performed += OnNavigate; // 入力があったらOnNavigateを実行
        submitAction.action.performed += OnSubmit;     // 入力があったらOnSubmitを実行
    }

    void OnDisable()
    {
        // メモリリーク防止
        if (navigateAction != null) navigateAction.action.performed -= OnNavigate;
        if (submitAction != null) submitAction.action.performed -= OnSubmit;
    }

    void Update()
    {
        // 毎フレーム、ポーズ切替ボタンが押されたか
        if (iaSettingtoggle.action.WasPressedThisFrame())
        {
            ToggleSettings();
        }
    }

    // 他のスクリプトから「今止まっているか」を確認するためのやつ
    public bool IsPaused() => isPaused;

    // --- 入力イベント（十字キー・スティック操作） ---
    private void OnNavigate(InputAction.CallbackContext context)
    {
        if (!isPaused) return; // ポーズ中以外は入力を受け付けない

        Vector2 direction = context.ReadValue<Vector2>();

        // --- 左右入力：メイン項目と閉じるボタンの行き来 ---
        // メインエリアにいる時に左(-0.5以下)を押したら「閉じるボタン」へ移動
        if (direction.x < -0.5f && !isCloseButtonSelected)
        {
            isCloseButtonSelected = true;
            ApplySelection();
        }
        // 閉じるボタンにいる時に右(0.5以上)を押したら「メインエリア」へ戻る
        else if (direction.x > 0.5f && isCloseButtonSelected)
        {
            isCloseButtonSelected = false;
            ApplySelection();
        }

        // --- 上下入力：メイン項目内での移動 ---
        if (!isCloseButtonSelected)
        {
            int move = 0;
            if (direction.y > 0.5f) move = -1;
            else if (direction.y < -0.5f) move = 1;

            if (move != 0)
            {
                int max = mainSettingButtons.Length;
                // インデックスをループさせる計算 (2の次は0, 0の次は2)
                currentIndex = (currentIndex + move + max) % max;
                ApplySelection();
            }
        }
    }

    // --- 入力イベント（決定キー操作） ---
    private void OnSubmit(InputAction.CallbackContext context)
    {
        // 突き抜け防止のため、ボタンを押し込んだ瞬間のみ実行
        if (!isPaused || !context.started) return;

        // 現在フォーカスがある方のOnClickイベントを実行する
        if (isCloseButtonSelected)
        {
            closeButton.onClick.Invoke();
        }
        else if (mainSettingButtons.Length > 0)
        {
            mainSettingButtons[currentIndex].onClick.Invoke();
        }
    }

    // --- ポーズの開閉切替 ---
    public void ToggleSettings()
    {
        if (isPaused) CloseSettings();
        else OpenSettings();
    }

    // --- シーン遷移用 ---
    public void ReturnTitle()
    {
        Time.timeScale = 1f; // 遷移前に時間を必ず戻す
        SceneManager.LoadScene("Scene_Title");
    }

    public void ReturnTutorial()
    {
        Time.timeScale = 1f; // 遷移前に時間を必ず戻す
        SceneManager.LoadScene("Scene_Tutorial");
    }

    // --- 内部ロジック：設定画面を開く ---
    private void OpenSettings()
    {
        if (settingUI != null) settingUI.SetActive(true);
        Time.timeScale = 0f; // ゲーム内の時間を停止

        // 指定されたスクリプトを無効化
        if (pauseTarget != null)
        {
            foreach (var target in pauseTarget)
                if (target != null) target.enabled = false;
        }

        // カーソル状態の保存と表示設定
        previousCursorVisible = Cursor.visible;
        previousLockMode = Cursor.lockState;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        isPaused = true;

        // 開いた直後の初期フォーカス設定：中央エリアの先頭
        isCloseButtonSelected = false;
        currentIndex = 0;
        ApplySelection();
    }

    // --- 内部ロジック：設定画面を閉じる ---
    public void CloseSettings()
    {
        if (settingUI != null) settingUI.SetActive(false);
        Time.timeScale = 1f; // ゲーム内の時間を再開

        // スクリプトを再び有効化
        if (pauseTarget != null)
        {
            foreach (var target in pauseTarget)
                if (target != null) target.enabled = true;
        }

        // カーソルの状態をポーズ前の状態に戻す
        Cursor.visible = previousCursorVisible;
        Cursor.lockState = previousLockMode;

        isPaused = false;
    }

    // --- 視覚的な選択状態の更新 ---
    void ApplySelection()
    {
        // UnityのEventSystemに「現在選択されているボタン」を伝える
        // これによりButtonコンポーネントの「Selected Color」などが反映される
        if (isCloseButtonSelected)
        {
            if (closeButton != null) closeButton.Select();
        }
        else
        {
            if (mainSettingButtons.Length > 0)
                mainSettingButtons[currentIndex].Select();
        }
    }
}