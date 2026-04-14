using UnityEngine;
using UnityEngine.InputSystem;

public class SC_Setting : MonoBehaviour
{
    [Header("Ref")]
    [Tooltip("ポーズ対象のスクリプトを指定")]
    [SerializeField] private MonoBehaviour[] pauseTarget;
    [Tooltip("設定用UI")]
    [SerializeField] private GameObject settingUI;
    [SerializeField] InputActionReference iaSettingtoggle;

    [Header("SeetingsData")]
    public bool CameraMode = false;

    private bool isPaused = false;
    private CursorLockMode previousLockMode;
    private bool previousCursorVisible;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(settingUI != null) CloseSettings();
    }

    // Update is called once per frame
    void Update()
    {
        var settingInput = iaSettingtoggle.action.WasPressedThisFrame();

        if (settingInput)
        {
            ToggleSettings();
        }
    }

    public void ToggleSettings()
    {
        if(isPaused)
        {
            CloseSettings();
        }
        else
        {
            OpenSettings();
        }
    }

    private void OpenSettings()
    {
        if(settingUI != null) settingUI.SetActive(true);

        Time.timeScale = 0f; // ゲームを一時停止

        if(pauseTarget != null)
        {
            foreach (var target in pauseTarget)
            {
                if (target != null) target.enabled = false; // 対象のスクリプトを無効化
            }
        }

        //元のカーソルの状態を保存
        previousCursorVisible = Cursor.visible;
        previousLockMode = Cursor.lockState;

        Cursor.visible = true; // カーソルを表示
        Cursor.lockState = CursorLockMode.None; // カーソルのロックを解除

        isPaused = true;
    }

    private void CloseSettings()
    {
        if (settingUI != null) settingUI.SetActive(false);

        Time.timeScale = 1f; // ゲームを再開

        if (pauseTarget != null)
        {
            foreach (var target in pauseTarget)
            {
                if (target != null) target.enabled = true; // 対象のスクリプトを有効化
            }
        }

        //元のカーソルの状態に戻す
        Cursor.visible = previousCursorVisible;
        Cursor.lockState = previousLockMode;

        isPaused = false;

    }
}
