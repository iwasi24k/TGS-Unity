using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SC_TitleManager : MonoBehaviour
{
    // --- インスペクターから設定する項目 ---
    [Header("Input Action References")]
    [SerializeField] private InputActionReference navigateAction; // 左右移動
    [SerializeField] private InputActionReference submitAction;   // 決定
    [SerializeField] private InputActionReference cancelAction;   // キャンセル（戻る）

    [Header("メインメニュー設定")]
    public Button[] mainButtons;       // 0:スタート, 1:終了
    public GameObject tutorialPopup;

    [Header("ポップアップメニュー設定")]
    public Button[] popupButtons;      // 0:はい, 1:いいえ

    [Header("遷移先シーン名")]
    public string mainGameSceneName = "GameScene";
    public string tutorialSceneName = "TutorialScene";

    // --- 内部変数 ---
    private int currentIndex = 0;       // 現在選択されているボタンの配列番号
    private bool isPopupActive = false; //

    // オブジェクトが有効になった時に呼ばれる
    private void OnEnable()
    {
        // アクションを有効化し、入力があった際に実行する関数（イベントハンドラー）を登録
        if (navigateAction != null)
        {
            navigateAction.action.Enable();
            navigateAction.action.performed += OnNavigate; // スティックを倒したりキーを押した時
        }
        if (submitAction != null)
        {
            submitAction.action.Enable();

            // startedを使って「押し込んだ瞬間」のみにして突き抜けを防いでいる(ゲームスタート→ポップアップのはいが連続でいかないようにしてる)
            submitAction.action.performed += OnSubmit;
        }
        if (cancelAction != null)
        {
            cancelAction.action.Enable();
            cancelAction.action.performed += OnCancel;
        }
    }

    // オブジェクトが無効（またはシーン遷移）になった時に呼ばれる
    private void OnDisable()
    {
        // 登録したイベントを解除（これを忘れると、消えたオブジェクトを操作しようとしてエラーになる）
        if (navigateAction != null) navigateAction.action.performed -= OnNavigate;
        if (submitAction != null) submitAction.action.performed -= OnSubmit;
        if (cancelAction != null) cancelAction.action.performed -= OnCancel;
    }

    void Start()
    {
        // 起動時に最初のボタンを「選択状態」にする
        ApplySelection();
    }

    // --- 入力イベントの処理 ---

    // 移動入力（矢印キーやスティック）があった時の処理
    private void OnNavigate(InputAction.CallbackContext context)
    {
        Vector2 direction = context.ReadValue<Vector2>();
        int move = 0;

        // デッドゾーンを考慮
        if (direction.x < -0.5f) move = -1;      // 左
        else if (direction.x > 0.5f) move = 1;   // 右

        if (move != 0)
        {
            // 操作中の配列（メインかポップアップか）に応じて最大数を切り替え
            int max = isPopupActive ? popupButtons.Length : mainButtons.Length;

            // インデックス計算（配列の範囲を超えないようにループさせる計算式）
            currentIndex = (currentIndex + move + max) % max;

            // 見た目を更新
            ApplySelection();
        }
    }

    /// 決定ボタンが押された時の処理
    public void OnSubmit(InputAction.CallbackContext context)
    {
        // started（押し込み開始）の時だけ実行し、ポップアップ開閉時の「連打」を防ぐ
        if (!context.started) return;

        Debug.Log("決定ボタンが押されました！ 現在のインデックス: " + currentIndex);

        // 現在操作しているグループのボタンを特定し、その「OnClick」イベントを無理やり実行する
        Button[] currentGroup = isPopupActive ? popupButtons : mainButtons;
        if (currentGroup.Length > 0)
        {
            currentGroup[currentIndex].onClick.Invoke();
        }
    }

    // キャンセルボタン（EscやBボタン）が押された時の処理
    public void OnCancel(InputAction.CallbackContext context)
    {
        ClosePopup();
    }

    // --- 各ボタンのインスペクター（OnClick）に登録する関数 ---
    public void OnClickStart() => OpenPopup();

    public void OnClickQuit()
    {
        // エディタ上なら再生停止、ビルド後ならアプリ終了
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }

    public void OnClickYes() => StartGame(true, tutorialSceneName);
    public void OnClickNo() => StartGame(false, mainGameSceneName);

    // ポップアップを開き、操作の焦点をポップアップへ移す
    void OpenPopup()
    {
        isPopupActive = true;
        currentIndex = 0; // ポップアップの最初の項目を選択
        tutorialPopup.SetActive(true);
        ApplySelection();
    }

    // ポップアップを閉じ、メインメニューに操作を戻す
    public void ClosePopup()
    {
        if (!isPopupActive) return;
        isPopupActive = false;
        currentIndex = 0;
        tutorialPopup.SetActive(false);
        ApplySelection();
    }

    // シーン遷移の共通処理
    void StartGame(bool skipTutorial, string sceneName)
    {
        // フラグを保存（1:行う, 0:スキップ）
        PlayerPrefs.SetInt("SkipTutorial", skipTutorial ? 1 : 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene(sceneName);
    }

    // UnityのEventSystemに対して、どのボタンが現在主役か伝える
    void ApplySelection()
    {
        Button[] currentGroup = isPopupActive ? popupButtons : mainButtons;
        if (currentGroup.Length > 0 && currentIndex < currentGroup.Length)
        {
            // 各ボタンのSelected Color適用
            currentGroup[currentIndex].Select();
        }
    }
}