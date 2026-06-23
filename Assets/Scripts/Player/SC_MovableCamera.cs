using UnityEngine;
using UnityEngine.InputSystem;

public class SC_MovableCamera : MonoBehaviour
{
    [Header("ターゲット設定")]
    [Tooltip("カメラが中心とする対象")]
    [SerializeField] private Transform target;

    [Tooltip("ターゲットの足元からの高さオフセット")]
    [SerializeField] private float targetHeightOffset = 1.5f;

    [Header("カメラ設定")]
    [Tooltip("ターゲットとの距離")]
    [SerializeField] private float distance = 5.0f;

    [Tooltip("横回転の感度")]
    [SerializeField] private float sensitivity = 0.5f;

    [Header("縦の角度")]
    [Tooltip("見下ろす角度")]
    [SerializeField] private float fixedYAngle = 20f;

    [Header("入力設定")]
    [Tooltip("カメラ操作用のアクションを割り当ててください")]
    [SerializeField] private InputActionReference lookAction;

    private float currentX = 0.0f;

    void Start()
    {
        // ゲーム開始時の初期の横角度を現在のカメラのY回転に合わせる
        currentX = transform.eulerAngles.y;
    }

    void OnEnable()
    {
        if (lookAction != null) lookAction.action.Enable();
    }

    void OnDisable()
    {
        if (lookAction != null) lookAction.action.Disable();
    }

    void LateUpdate()
    {
        if (target == null || lookAction == null) return;

        // 入力値の取得
        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();

        // X軸のみを使用して回転角度を更新
        currentX += lookInput.x * sensitivity;

        // クォータニオンで新しい回転を計算
        Quaternion rotation = Quaternion.Euler(fixedYAngle, currentX, 0);

        // ターゲットの座標に高さをプラスした中心点を計算
        Vector3 targetPosition = target.position + Vector3.up * targetHeightOffset;

        // カメラの位置を計算
        Vector3 position = targetPosition - (rotation * Vector3.forward * distance);

        // カメラに位置と回転を適用
        transform.position = position;
        transform.rotation = rotation;
    }
}